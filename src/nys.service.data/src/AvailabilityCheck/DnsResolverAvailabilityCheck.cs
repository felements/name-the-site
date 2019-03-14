using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace nys.service.data.AvailabilityCheck
{
    internal class DnsResolverAvailabilityCheck : IAvailabilityStatusProvider
    {
        private readonly ILogger<DnsResolverAvailabilityCheck> _logger;
        private readonly ConcurrentBag<string> _unavailableNames = new ConcurrentBag<string>();

        public DnsResolverAvailabilityCheck(ILogger<DnsResolverAvailabilityCheck> logger)
        {
            _logger = logger;
        }
        
        public async Task<bool> IsAvailable(string name)
        {
            if (_unavailableNames.Contains(name)) return false;

            bool result = false;
            try
            {
                var entry = await Dns.GetHostEntryAsync(name);
                result = !entry.AddressList.Any();
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.HostNotFound) result = true;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Error while checking name '{name}'");
                result = true;
            }

            if (!result) _unavailableNames.Add(name);
            _logger.LogDebug($"Domain {name}, is {(result ? "" : "NOT")} available.");

            return result;
        }
    }
}