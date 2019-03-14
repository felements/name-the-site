using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;

namespace nys.service.data.NameProvider
{
    public class SlangdefineTitleProvider : INamePartProvider, IDisposable
    {
        private readonly ILogger<SlangdefineTitleProvider> _logger;
        private const string SlangDefineSiteUrl = "https://slangdefine.org/";
        private const string SlangDefineResource = "top100.php";
        private static readonly Regex Expression = new Regex("\"list-group-item\"\\>\\d+\\.\\s+(?<word>\\w+\\s*\\w*)\\<", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        
        private readonly ConcurrentBag<string> _cache = new ConcurrentBag<string>();
        private DateTime _lastRenewUtc;
        private const int CacheLifetimeHours = 12;
        private readonly ManualResetEventSlim _cacheRenewMre = new ManualResetEventSlim(true);
        private readonly CancellationTokenSource _cacheRenewCts = new CancellationTokenSource();
        private readonly AsyncPolicy _cacheRenewPolicy = Policy.WrapAsync(
            Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))),
            Policy.TimeoutAsync(TimeSpan.FromSeconds(20)));
        

        public SlangdefineTitleProvider(ILogger<SlangdefineTitleProvider> logger)
        {
            _logger = logger;

            Task.Run(async () =>
            {
                while (!_cacheRenewCts.IsCancellationRequested)
                {
                    _cacheRenewMre.Wait(_cacheRenewCts.Token);
                    try
                    {
                        if (_cacheRenewCts.IsCancellationRequested) return;

                        var data =  await _cacheRenewPolicy.ExecuteAsync(async ctx => await FetchAsync(Parse, ctx), _cacheRenewCts.Token);
                        if (data.Any())
                        {
                            _cache.Clear();
                            data.ToList().ForEach(d => _cache.Add(d));
                            _lastRenewUtc = DateTime.UtcNow;
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Cache renew loop");
                    }
                    finally
                    {
                        _cacheRenewMre.Reset();
                    }
                }
            }, _cacheRenewCts.Token);
        }
        
        public Task<string[]> GetAsync(CancellationToken cancellationToken)
        {
            if ((DateTime.UtcNow - _lastRenewUtc).TotalHours > CacheLifetimeHours)
            {
                _cacheRenewMre.Set();
            }

            return Task.FromResult(_cache.ToArray());
        }

        private async Task<string[]> FetchAsync(Func<string, string[]> parser,  CancellationToken cancellationToken)
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    MaxConnectionsPerServer = 5 
                };
                var client = new HttpClient(handler) {BaseAddress = new Uri(SlangDefineSiteUrl), Timeout = TimeSpan.FromSeconds(10)};
                var request = new HttpRequestMessage(HttpMethod.Get, SlangDefineResource);

                var response = await client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var data =  parser(content);
                _logger.LogDebug($"Got {data.Length} variants from {SlangDefineSiteUrl}");
                return data;
            }
            catch (HttpRequestException httpException)
            {
                _logger.LogWarning(httpException, $"Unable to get data from {SlangDefineSiteUrl}");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Something went wrong");
                throw;
            }
        }
        
        private static string[] Parse(string content)
        {
            return string.IsNullOrWhiteSpace(content) ? new string[]{} : Expression.Matches(content).Select(x => x.Groups["word"].Value).ToArray();
        }

        public void Dispose()
        {
            _cacheRenewCts.Cancel();
            _cacheRenewMre.Set();
            _cacheRenewMre?.Dispose();
            _cacheRenewCts?.Dispose();
        }
    }
}