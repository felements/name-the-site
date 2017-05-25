using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using nys.misc;
using nys.web.api.Services.DomainAvailabilityCheck;
using nys.web.api.Services.WordVariants;
using NLog;

namespace nys.web.api.Services.NameGenerator
{
    public class RandomNameGeneratorService : INameGeneratorService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const byte MaxRetryCount = 10;

        private readonly IWordVariantsService _titleVariantsService;
        private readonly IWordVariantsService _zoneVariantsService;
        private readonly IDomainAvailabilityCheckService _availabilityCheckService;

        public RandomNameGeneratorService(
            IIndex<WordVariantType, IWordVariantsService> wordServices,
            IDomainAvailabilityCheckService availabilityCheckService )
        {
            _titleVariantsService = wordServices[WordVariantType.Title];
            _zoneVariantsService = wordServices[WordVariantType.Zone];
            _availabilityCheckService = availabilityCheckService;
        }

        public async Task<string> Generate()
        {
            var words = await _titleVariantsService.GetAsync();
            var zones = await _zoneVariantsService.GetAsync();

            byte attempts = 0;
            while (attempts <= MaxRetryCount)
            {
                attempts++;

                var suggestedName = Combine(words, zones);
                var available = await _availabilityCheckService.Check(suggestedName);

                if (available)
                {
                    Logger.Debug("Found available domain name {0} after {1} attempts", suggestedName.ToUpperInvariant(), attempts);
                    return suggestedName;
                }
            }

            Logger.Warn("Could not find the available domain name after {0} attempts", attempts);
            return null;
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