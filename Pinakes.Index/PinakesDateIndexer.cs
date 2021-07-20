using Fusi.Antiquity.Chronology;
using Fusi.Tools;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;

namespace Pinakes.Index
{
    /// <summary>
    /// Pinakes date indexer. This collects century indications from authors
    /// and works and stores them into the <c>date</c> table.
    /// </summary>
    public sealed class PinakesDateIndexer : PinakesIndexer
    {
        private readonly PinakesCenturyDateAdapter _adapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PinakesDateIndexer"/>
        /// class.
        /// </summary>
        /// <param name="connString">The connection string.</param>
        public PinakesDateIndexer(string connString) : base(connString)
        {
            _adapter = new PinakesCenturyDateAdapter();
        }

        private static void InitTarget(IDbConnection connection)
        {
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = LoadResourceText("Date.mysql");
            cmd.ExecuteNonQuery();

            cmd.CommandText = "TRUNCATE TABLE pix_date;";
            cmd.ExecuteNonQuery();
        }

        private static IList<Tuple<int, string>> CollectSources(
            IDbConnection connection,
            string table)
        {
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT id, siecle FROM {table} " +
                "WHERE siecle IS NOT NULL AND LENGTH(siecle) > 0;";
            using IDataReader reader = cmd.ExecuteReader();

            List<Tuple<int, string>> sources = new List<Tuple<int, string>>();
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string source = reader.GetString(1);
                sources.Add(Tuple.Create(id, source));
            }
            return sources;
        }

        private void WriteDates(IList<Tuple<int, string>> sources,
            IDbCommand command,
            string field,
            CancellationToken cancel,
            IProgress<ProgressReport> progress = null)
        {
            ProgressReport report = progress != null ? new ProgressReport() : null;
            foreach (var s in sources)
            {
                HistoricalDate date = _adapter.GetDate(s.Item2);
                if (date == null)
                {
                    Logger?.LogError(
                        $"Invalid date at {field}#{s.Item1}: \"{s.Item2}\"");
                    continue;
                }

                ((DbParameter)command.Parameters["@field"]).Value = field;
                ((DbParameter)command.Parameters["@target_id"]).Value = s.Item1;
                ((DbParameter)command.Parameters["@date_txt"]).Value = date.ToString();
                ((DbParameter)command.Parameters["@date_val"]).Value = date.GetSortValue();
                ((DbParameter)command.Parameters["@source"]).Value = s.Item2;
                command.ExecuteNonQuery();

                if (cancel.IsCancellationRequested) break;
                if (progress != null && ++report.Count % 10 == 0)
                {
                    report.Percent = report.Count * 100 / sources.Count;
                    progress?.Report(report);
                }
            }

            if (progress != null)
            {
                report.Percent = 100;
                progress?.Report(report);
            }
        }

        /// <summary>
        /// Perform the indexing.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="cancel">The cancel token.</param>
        /// <param name="progress">The optional progress reporter.</param>
        /// <exception cref="ArgumentNullException">connection</exception>
        protected override void DoIndex(IDbConnection connection,
            CancellationToken cancel,
            IProgress<ProgressReport> progress = null)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            // create or truncate date
            InitTarget(connection);

            // insert data from authors and works
            const string parameters = "@field,@target_id,@date_txt,@date_val,@source";
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO `pix_date`" +
                "(`field`,`target_id`,`date_txt`,`date_val`,`source`) " +
                $"VALUES({parameters});";
            foreach (string p in parameters.Split(','))
                AddParameter(p, cmd, p == "@date_val" ? DbType.Double : DbType.String);

            var sources = CollectSources(connection, "auteurs");
            WriteDates(sources, cmd, "aut", cancel, progress);

            sources = CollectSources(connection, "oeuvres");
            WriteDates(sources, cmd, "wrk", cancel, progress);
        }
    }
}
