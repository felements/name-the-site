using System;
using System.Globalization;
using System.IO;

namespace nys.misc
{
    public static class ApplicationExtensions
    {
        private static readonly string Version;
        static ApplicationExtensions()
        {
#if DEBUG
            Version = "dev." + Guid.NewGuid().ToString("N").Substring(0, 4);
#else
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            Version = string.Format("{0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);
#endif

        }

        public static ApplicationEnvironment GetEnvironment()
        {
            if (Type.GetType("Mono.Runtime") != null)
            {
                return ApplicationEnvironment.Mono;
            }
            return ApplicationEnvironment.WindowsNative;
        }

        public static string GetVersion()
        {
            return Version;
        }

        public static string CreateStorePath(Guid id, string storePath, string filename)
        {
            var storeFileName = id.ToString("D") + Path.GetExtension(filename ?? string.Empty);
            var path = Environment.CurrentDirectory
                + storePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar)
                + storeFileName;

            return path;
        }

        public static CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");
    }
}