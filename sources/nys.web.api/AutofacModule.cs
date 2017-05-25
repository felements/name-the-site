using Autofac;
using nys.web.api.Services.DomainAvailabilityCheck;
using nys.web.api.Services.NameGenerator;
using nys.web.api.Services.WordVariants;
using NLog;

namespace nys.web.api
{
    public class AutofacModule : Module
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected override void Load(ContainerBuilder builder)
        {
            Logger.Debug("Register " + typeof(SlangTitleService).Name);
            builder.RegisterType<SlangTitleService>()
                .Keyed<IWordVariantsService>(WordVariantType.Title)
                .InstancePerLifetimeScope();

            Logger.Debug("Register " + typeof(StaticDomainZoneService).Name);
            builder.RegisterType<StaticDomainZoneService>()
                .Keyed<IWordVariantsService>(WordVariantType.Zone)
                .InstancePerLifetimeScope();
            
            Logger.Debug("Register " + typeof(RandomNameGeneratorService).Name);
            builder.RegisterType<RandomNameGeneratorService>()
                .As<INameGeneratorService>()
                .InstancePerLifetimeScope();
            
            Logger.Debug("Register " + typeof(DnsResolveAvailabilityCheck).Name);
            builder.RegisterType<DnsResolveAvailabilityCheck>()
                .As<IDomainAvailabilityCheckService>()
                .InstancePerLifetimeScope();
        }
    }
}