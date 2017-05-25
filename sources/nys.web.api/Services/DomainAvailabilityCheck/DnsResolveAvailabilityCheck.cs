using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NLog;

namespace nys.web.api.Services.DomainAvailabilityCheck
{
    public class DnsResolveAvailabilityCheck : IDomainAvailabilityCheckService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Lazy<ConcurrentDictionary<string, string>> NotAwailableDomainNames =
            new Lazy<ConcurrentDictionary<string, string>>();

        public async Task<bool> Check(string name)
        {
            if (NotAwailableDomainNames.Value.ContainsKey(name)) return false;

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
                Logger.Error(e);
                result = true;
            }

            if (!result) NotAwailableDomainNames.Value.TryAdd(name, name);
            Logger.Debug("Domain {0}, is available: {1}. Information from DNS Resolver", name, result);

            return result;
        }
    }
}