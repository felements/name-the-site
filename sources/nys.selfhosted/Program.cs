using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime;
using Mono.Unix;
using Mono.Unix.Native;
using nys.misc;
using Nancy.Hosting.Self;
using NLog;

namespace nys.selfhosted
{
    class Program
    {
        private static Logger _logger;
        private static NancyHost _host;

        static void Main(string[] args)
        {
            var timer = Stopwatch.StartNew();
            _logger = LogManager.GetCurrentClassLogger();

            var hostUrl = "http://localhost:4455";

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                _logger.Debug("");
                _logger.Debug("");
                _logger.Debug("*******************************************");
                _logger.Debug("START <{0}>", assembly.FullName);
                _logger.Debug("URL : {0}", hostUrl);
                _logger.Debug("Environment : {0}, {1}",
                    ApplicationExtensions.GetEnvironment().ToString("G"),
                    Environment.Version);
                _logger.Debug("*******************************************");
                _logger.Debug("GC mode: {0}", GCSettings.IsServerGC ? "SERVER" : "DESKTOP");


                _logger.Debug("Trying to start host..");
                var hostConfiguration = new HostConfiguration
                {
                    UrlReservations = { CreateAutomatically = true },
                    UnhandledExceptionCallback = exception =>
                    {
                        if (exception is HttpListenerException) return;

                        Console.WriteLine(exception.ToString());
                        _logger.Fatal(exception);
                    }
                };
                _host = new NancyHost(hostConfiguration, new Uri(hostUrl));
                _host.Start();
                _logger.Debug("Host is started successfully.");
                _logger.Debug("*******************************************");
                _logger.Debug("APPLICATION INITIALIZED SUCCESSFULLY");
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex);
                Console.WriteLine("{0} - {1}", ex.GetType().Name, ex.Message);
            }
            finally
            {
                timer.Stop();
                _logger.Debug("Elapsed time for application initialization (milliseconds): {0}",
                    timer.ElapsedMilliseconds);
                _logger.Debug("*******************************************");
            }

            try
            {
                //
                // wait for termination
                var env = ApplicationExtensions.GetEnvironment();
                Console.WriteLine("Host is started successfully.");
                switch (env)
                {
                    case ApplicationEnvironment.Mono:
                        // on mono, processes will usually run as daemons - this allows you to listen
                        // for termination signals (ctrl+c, shutdown, etc) and finalize correctly
                        UnixSignal.WaitAny(new[]
                        {
                            new UnixSignal(Signum.SIGINT),
                            new UnixSignal(Signum.SIGTERM),
                            new UnixSignal(Signum.SIGQUIT),
                            new UnixSignal(Signum.SIGHUP)
                        });
                        break;
                    case ApplicationEnvironment.Unknown:
                    case ApplicationEnvironment.WindowsNative:
                    case ApplicationEnvironment.WindowsCore:
#if DEBUG
                        RunBrowser(hostUrl);
#endif
                        Console.WriteLine("Press [Enter] to close the host.");
                        Console.ReadLine();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _logger.Debug("STOP ");
                _host.Stop();
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex);
                Console.WriteLine("{0} - {1}", ex.GetType().Name, ex.Message);
            }
        }

        private const string BrowserBinaryPath = "c:\\Program Files (x86)\\Mozilla Firefox\\firefox.exe";
        private static void RunBrowser(string url)
        {
            if (!File.Exists(BrowserBinaryPath)) return;
            
            Process.Start(BrowserBinaryPath, "-new-tab " + url);
        }
    }
}
