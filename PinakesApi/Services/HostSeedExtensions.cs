using Fusi.DbManager.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace PinakesApi.Services
{
    /// <summary>
    /// Host seed extensions.
    /// </summary>
    public static class HostSeedExtensions
    {
        private static string LoadResourceText(string name)
        {
            using StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    $"PinakesApi.Assets.{name}"), Encoding.UTF8);
            return reader.ReadToEnd();
        }

        private static void EnsureDatabaseExists(string name,
            IConfiguration config,
            IHostEnvironment environment)
        {
            // build connection string to it
            string cs = string.Format(
                CultureInfo.InvariantCulture,
                config.GetConnectionString("Default"),
                name);

            // check if DB exists
            Serilog.Log.Information($"Checking for database {name}...");

            MySqlDbManager manager = new MySqlDbManager(cs);
            if (!manager.Exists(name))
            {
                Serilog.Log.Information($"Creating database {name}...");

                manager.CreateDatabase(name,
                    LoadResourceText("Schema.mysql"), null);

                Serilog.Log.Information("Database created.");

                // import
                string path = Path.Combine(
                    config.GetValue<string>("Data:SourceDir"),
                    "data.zip");
                Serilog.Log.Information("ZIP data path: " + path);

                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    Serilog.Log.Information($"Importing database from {path}...");
                    MySqlImportService service = new MySqlImportService();
                    service.Import(path, cs);
                    Serilog.Log.Information("Import completed.");
                }
            }
        }

        private static void ImportDatabase(IServiceProvider serviceProvider)
        {
            Policy.Handle<DbException>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(60)
                }, (exception, timeSpan, _) =>
                {
                    // in case of DbException we must retry
                    ILogger logger = serviceProvider
                        .GetService<ILoggerFactory>()
                        .CreateLogger(typeof(HostSeedExtensions));

                    string message = "Unable to connect to DB" +
                        $" (sleep {timeSpan}): {exception.Message}";
                    logger.LogError(exception, message);
                }).Execute(() =>
                {
                    IConfiguration config =
                        serviceProvider.GetService<IConfiguration>();
                    IHostEnvironment environment =
                        serviceProvider.GetService<IHostEnvironment>();
                    ILogger logger = serviceProvider
                        .GetService<ILoggerFactory>()
                        .CreateLogger(typeof(HostSeedExtensions));

                    // delay if requested, to allow DB start
                    int delay = config.GetValue<int>("Seed:Delay");
                    if (delay > 0)
                    {
                        logger.LogInformation($"Waiting for {delay} seconds...");
                        Thread.Sleep(delay * 1000);
                    }
                    else logger.LogInformation("No delay for seeding");

                    EnsureDatabaseExists("pinakes", config, environment);
                });
        }

        /// <summary>
        /// Seeds the database.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns>The received host, to allow concatenation.</returns>
        /// <exception cref="ArgumentNullException">serviceProvider</exception>
        public static IHost Seed(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                IServiceProvider serviceProvider = scope.ServiceProvider;
                ILogger logger = serviceProvider
                    .GetService<ILoggerFactory>()
                    .CreateLogger(typeof(HostSeedExtensions));

                try
                {
                    IConfiguration config =
                        serviceProvider.GetService<IConfiguration>();

                    ImportDatabase(serviceProvider);
                }
                catch (Exception ex)
                {
                    // Console.WriteLine(ex.ToString());
                    logger.LogError(ex, ex.Message);
                    throw;
                }
            }
            return host;
        }
    }
}
