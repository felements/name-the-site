using System.IO;
using Autofac;
using nys.misc;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Conventions;
using Nancy.Diagnostics;
using Nancy.Routing;
using Nancy.Security;
using NLog;

namespace nys.selfhosted
{
    public class Bootstrapper : AutofacNancyBootstrapper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected override void ConfigureApplicationContainer(ILifetimeScope existingContainer)
        {
            Logger.Debug("*******************************************");
            Logger.Debug("***** Application container registrations");

            Logger.Debug("Force loading referenced assemblies");
            AssemblyExtensions.ForceLoadAssemblies(misc.Constants.Namespace.AssemblyNamePrefix);

            var builder = new ContainerBuilder();
            foreach (var assembly in AssemblyExtensions.GetAssemblies(misc.Constants.Namespace.AssemblyNamePrefix))
            {
                Logger.Debug("> Register Autofac modules in " + assembly.FullName);
                builder.RegisterAssemblyModules(assembly);
            }

            builder.Update(existingContainer.ComponentRegistry);
        }
        

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            Logger.Debug("NancyFx: enabled schema rewrite");
            SSLProxy.RewriteSchemeUsingForwardedHeaders(pipelines);

            Logger.Debug("NancyFx: enabled CSRF protection");
            Csrf.Enable(pipelines);
            
            Logger.Debug("NancyFx: diagnostics is disabled");
            DiagnosticsHook.Disable(pipelines);
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

            nancyConventions.StaticContentsConventions.Clear();
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("content"));
        }

    }
}
