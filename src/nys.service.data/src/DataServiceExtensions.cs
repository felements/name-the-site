using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using nys.service.data.AvailabilityCheck;
using nys.service.data.NameProvider;

namespace nys.service.data
{
    public static class DataServiceExtensions
    {
        public static void AddNameSuggestionServices(this IServiceCollection services)
        {
            services.AddSingleton<IAvailabilityStatusProvider, DnsResolverAvailabilityCheck>();
            
            services.AddSingleton<StaticDomainZoneProvider>();
            services.AddSingleton<StaticTitleProvider>();
            services.AddSingleton<SlangdefineTitleProvider>();

            services.AddSingleton<INameProvider, RandomNameLinker>(provider =>
                new RandomNameLinker(
                    provider.GetRequiredService<ILogger<RandomNameLinker>>(),
                    provider.GetRequiredService<IAvailabilityStatusProvider>(),
                    type =>
                    {
                        switch (type)
                        {
                            case NamePartType.Title:
                                return new INamePartProvider[]
                                {
                                    provider.GetRequiredService<SlangdefineTitleProvider>(),
                                    provider.GetRequiredService<StaticTitleProvider>(),
                                    
                                } ;
                            case NamePartType.Zone:
                                return new INamePartProvider[]
                                {
                                    provider.GetRequiredService<StaticDomainZoneProvider>()
                                };
                            default:
                                throw new ArgumentOutOfRangeException(nameof(type));
                        }
                    }));
        }
    }
}