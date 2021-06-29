using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pinix.Cli
{
    // using Microsoft.Extensions.CommandlineUtils:
    // https://gist.github.com/iamarcel/8047384bfbe9941e52817cf14a79dc34
    // console app structure:
    // https://github.com/iamarcel/dotnet-core-neat-console-starter

    // If you need MS configuration, add these packages:
    //   Install-Package Microsoft.Extensions.Configuration
    //   Install-Package Microsoft.Extensions.Configuration.Binder
    //   Install-Package Microsoft.Extensions.Configuration.FileExtensions
    //   Install-Package Microsoft.Extensions.Configuration.Json
    // The Binder is required for Get<T> extension method on IConfiguration,
    // i.e. getting settings POCO objects.
    // You can get a configuration object like this:
    // IConfigurationBuilder builder = new ConfigurationBuilder()
    //   .SetBasePath(Directory.GetCurrentDirectory())
    //   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    // _configuration = builder.Build();
    // In this sample you must add appsettings.json as Content/copy if newer
    // to the project.

    public static class Program
    {
#if DEBUG
        private static void DeleteLogs()
        {
            foreach (var path in Directory.EnumerateFiles(
                AppDomain.CurrentDomain.BaseDirectory, "pinix-log*.txt"))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
#endif

        public static int Main(string[] args)
        {
            try
            {
                // https://github.com/serilog/serilog-sinks-file
                string logFilePath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "pinix-log.txt");
                Log.Logger = new LoggerConfiguration()
#if DEBUG
                    .MinimumLevel.Debug()
#else
                    .MinimumLevel.Information()
#endif
                    .Enrich.FromLogContext()
                    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
                    .CreateLogger();

#if DEBUG
                DeleteLogs();
#endif

                Console.OutputEncoding = Encoding.Unicode;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                Task.Run(async () =>
                {
                    AppOptions options = AppOptions.Parse(args);
                    if (options?.Command == null)
                    {
                        // RootCommand will have printed help
                        return 1;
                    }

                    Console.Clear();
                    await options.Command.Run();
                    return 0;
                }).Wait();

                Console.ResetColor();
                Console.CursorVisible = true;
                Console.WriteLine();
                Console.WriteLine();

                stopwatch.Stop();
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    Console.WriteLine("\nTime: {0}d{1}h{2}'{3}\"",
                        stopwatch.Elapsed.Days,
                        stopwatch.Elapsed.Hours,
                        stopwatch.Elapsed.Minutes,
                        stopwatch.Elapsed.Seconds);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Console.CursorVisible = true;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();
                return 2;
            }
        }
    }
}
