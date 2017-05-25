using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using nys.misc;
using NLog;
using RestSharp;

namespace nys.web.api.Services.WordVariants
{
    public class SlangTitleService : IWordVariantsService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string SlangDefineSiteUrl = "https://slangdefine.org/";
        private const string SlangDefineResource = "top100.php";
        private static readonly Regex Expression = new Regex("\"list-group-item\"\\>\\d+\\.\\s+(?<word>\\w+\\s*\\w*)\\<", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant | RegexOptions.Compiled);


        private static object _wordsLock = new object();
        private static DateTime _wordsFetchTimeUtc = DateTime.UtcNow;
        private static List<string> _words = new List<string>(100);
        private const byte _wordsLifetimeHours = 12;

        

        public async Task<string[]> GetAsync()
        {
            lock (_wordsLock)
            {
                if ((DateTime.UtcNow - _wordsFetchTimeUtc).TotalHours > _wordsLifetimeHours)
                {
                    _words.Clear();
                }
            }

            if (_words.Any()) return _words.ToArray();
            lock (_wordsLock)
            {
                if (_words.Any()) return _words.ToArray();

                _words = Load();
                _wordsFetchTimeUtc = DateTime.UtcNow;
            }
            return _words.ToArray();
        }


        private List<string> Load()
        {
            try
            {
                var client = new RestClient(SlangDefineSiteUrl);
                var request = new RestRequest(SlangDefineResource, Method.GET);

                // execute the request
                var response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Logger.Warn("Bad response from {1}: {0} - {2}", response.StatusCode, SlangDefineSiteUrl,
                        response.StatusDescription);
                    return new List<string>();
                }
                var content = response.Content; // raw content as string

                var words = Parse(content);
                Logger.Debug("Got {0} words from {1}", words.Count, SlangDefineSiteUrl);
                
                return words;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return new List<string>();
            }
        }

        private static List<string> Parse(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return new List<string>();

            var matches = Expression.Matches(content).Cast<Match>().AsParallel().Select(x => x.Groups["word"].Value).ToList();
            return matches;
        }
    }
}