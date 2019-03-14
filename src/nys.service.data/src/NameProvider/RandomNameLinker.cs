using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using nys.service.data.AvailabilityCheck;

namespace nys.service.data.NameProvider
{
    internal class RandomNameLinker : INameProvider
    {
        private const int MaxResultCount = 100;
        private const byte MaxRetryCount = 10;
        
        private readonly ILogger<RandomNameLinker> _logger;
        private readonly INamePartProvider[] _titleVariantsProviders;
        private readonly INamePartProvider[] _zoneVariantsProviders;
        private readonly IAvailabilityStatusProvider _availabilityCheckService;

        public RandomNameLinker(
            ILogger<RandomNameLinker> logger,
            IAvailabilityStatusProvider availabilityCheckService,
            Func<NamePartType, INamePartProvider[]> namePartProviderFactory )
        {
            _logger = logger;
            _availabilityCheckService = availabilityCheckService;
            _titleVariantsProviders = namePartProviderFactory(NamePartType.Title);
            _zoneVariantsProviders = namePartProviderFactory(NamePartType.Zone);
        }

        public async Task<string[]> GetNext(int count, CancellationToken ct)
        {
            int normalizedCount = Math.Min( Math.Abs(count), MaxResultCount );
            
            //
            //  Get variants
            
            _logger.LogDebug($"Generating next {normalizedCount} addresses");
            
            var words = (await Task.WhenAll(_titleVariantsProviders.Select(p => p.GetAsync(ct)))).SelectMany(r=>r).ToArray();
            _logger.LogDebug($"Got {words.Length} parts from TITLE providers");
            
            var zones = (await Task.WhenAll(_zoneVariantsProviders.Select(p => p.GetAsync(ct)))).SelectMany(r=>r).ToArray();
            _logger.LogDebug($"Got {zones.Length} parts from ZONE providers");

            // 
            // Combine variants

            var result = new List<string>(normalizedCount);
            int attemptsCount = 0;
            while (attemptsCount <= MaxRetryCount && result.Count < normalizedCount)
            {
                attemptsCount++;

                var suggestedName = Combine(words, zones);
                var available = await _availabilityCheckService.IsAvailable(suggestedName);

                if (!available) continue;
                
                //_logger.LogDebug($"Found available domain name {suggestedName.ToUpperInvariant()} after {attemptsCount} attempts");
                result.Add(suggestedName);
            }

            _logger.Log( result.Count == normalizedCount ? LogLevel.Debug : LogLevel.Warning, 
                $"Found {result.Count} of {normalizedCount} - {string.Join(' ', result)}");
            return result.ToArray();
        }


        private static string Combine(string[] words, string[] zones)
        {
            var rnd = RandomProvider.GetThreadRandom();

            var word = words.OrderBy(itm=>rnd.Next()).ToArray()[rnd.Next(words.Length - 1)];
            var zone = zones.OrderBy(itm => rnd.Next()).ToArray()[rnd.Next(zones.Length - 1)];

            return string.Concat(word.Replace(" ", "-"), ".", zone).ToLowerInvariant();
        }

    }
}