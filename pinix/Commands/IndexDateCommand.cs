using Fusi.Tools;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pinakes.Index;
using ShellProgressBar;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Pinix.Cli.Commands
{
    public sealed class IndexDateCommand : ICommand
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly string _dbName;

        public IndexDateCommand(AppOptions options, string dbName)
        {
            _config = options.Configuration;
            _logger = options.Logger;
            _dbName = dbName ?? "pinakes";
        }

        public static void Configure(CommandLineApplication command,
            AppOptions options)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            command.Description = "Index the dates for authors and works " +
                "into the date table, creating or truncating it.";
            command.HelpOption("-?|-h|--help");

            CommandArgument dbArgument = command.Argument("[database]",
                "The name of the database");

            command.OnExecute(() =>
            {
                options.Command = new IndexDateCommand(
                    options,
                    dbArgument.Value);
                return 0;
            });
        }

        public Task Run()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nINDEX DATE\n");
            Console.ResetColor();
            Console.WriteLine($"Database name: {_dbName}\n");

            string connString = string.Format(CultureInfo.InvariantCulture,
                _config.GetConnectionString("Default"),
                _dbName);

            _logger?.LogInformation("Index date");
            ProgressBar bar = new ProgressBar(100, null, new ProgressBarOptions
            {
                DisplayTimeInRealTime = true,
                EnableTaskBarProgress = true
            });
            PinakesDateIndexer indexer = new PinakesDateIndexer(connString)
            {
                Logger = _logger
            };
            indexer.Index(CancellationToken.None, new Progress<ProgressReport>(
                report =>
            {
                bar.Tick(report.Percent);
            }));

            return Task.CompletedTask;
        }
    }
}
