using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using NLog;
using RestSharp;

namespace nys.web.api.Services.DomainAvailabilityCheck
{
    public class WhoisDomainAvailabilityCheck : IDomainAvailabilityCheckService
    {
        private const string NicRuUrl = "https://www.nic.ru/";
        private const string NicRuResource = "whois";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Lazy<ConcurrentDictionary<string, string>> NotAwailableDomainNames =
            new Lazy<ConcurrentDictionary<string, string>>();

        public async Task<bool> Check(string name)
        {
            if (NotAwailableDomainNames.Value.ContainsKey(name)) return false;

            var client = new RestClient(NicRuUrl);
            var request = new RestRequest(NicRuResource, Method.GET);
            request.AddParameter("query", name);

            try
            {
                // execute the request
                var response = await client.ExecuteTaskAsync(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Logger.Warn("Bad response from {1}: {0} - {2}", response.StatusCode, NicRuUrl,
                        response.StatusDescription);
                    return false;
                }
                var content = response.Content; // raw content as string
                var isAvailable = Parse(content);

                Logger.Debug("Domain {0}, is available: {1}. Information from {2}", name, isAvailable, NicRuUrl);
                if (!isAvailable) NotAwailableDomainNames.Value.TryAdd(name, name);

                return isAvailable;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        private static bool Parse(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return false;
            if (content.Contains("омен занят")) return false; //todo: find the API service and use it

            return true;
        }
    }
}