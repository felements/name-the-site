using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace nys.misc
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetDerivedTypes<TBase>(string assembliesNamePrefix)
        {
            var assemblies = GetAssemblies(assembliesNamePrefix);

            var result = new List<Type>();
            foreach (var assembly in assemblies)
            {
                result.AddRange(assembly
                    .GetExportedTypes()
                    .Where(tp => typeof(TBase).IsAssignableFrom(tp) && tp != typeof(TBase)));
            }
            return result;
        }

        public static IEnumerable<Assembly> GetAssemblies(string assembliesNamePrefix)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName.StartsWith(assembliesNamePrefix, StringComparison.InvariantCultureIgnoreCase));
            return assemblies;
        }

        public static void ForceLoadAssemblies(string assemblyNamePrefix)
        {
            try
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                var loadedPaths = loadedAssemblies
                    .Where(x => x.FullName.StartsWith(assemblyNamePrefix))
                    .Select(a => a.Location).ToArray();

                var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
                var toLoad = referencedPaths
                    .Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase))
                    .Where(
                        x =>
                            Path.GetFileName(x)
                                .StartsWith(assemblyNamePrefix, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
                toLoad.ForEach(path =>
                {
                    try
                    {
                        AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path));
                    }
                    catch (Exception e)
                    {
                    }
                });
            }
            catch (Exception e)
            {

            }
        }

        public static void ActionPerInstance<TBase>(this IEnumerable<Type> types, Action<TBase> action)
        {
            if (types == null) return;
            foreach (var type in types)
            {
                var instance = (TBase)Activator.CreateInstance(type);
                action(instance);
            }
        }
    }
}
