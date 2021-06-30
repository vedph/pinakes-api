using Fusi.Antiquity.Chronology;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Pinakes.Index
{
    /// <summary>
    /// Pinakes date indexer. This collects century indications from authors
    /// and works and stores them into the <c>date</c> table.
    /// </summary>
    public sealed class PinakesDateIndexer
    {
        private readonly string _connString;
        private readonly PinakesCenturyDateAdapter _adapter;

        public PinakesDateIndexer(string connString)
        {
            _connString = connString
                ?? throw new ArgumentNullException(nameof(connString));
            _adapter = new PinakesCenturyDateAdapter();
        }

        private static string LoadResourceText(string name)
        {
            using (StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    $"Pinakes.Index.Assets.{name}"), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private static void InitTarget(IDbConnection connection)
        {
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = LoadResourceText("Schema.mysql");
            cmd.ExecuteNonQuery();

            cmd.CommandText = "TRUNCATE TABLE date;";
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

        private static void AddParameter(string name, IDbCommand command,
            DbType type)
        {
            var p = command.CreateParameter();
            p.DbType = type;
            p.ParameterName = name;
            p.Direction = ParameterDirection.Input;
            command.Parameters.Add(p);
        }

        private void WriteDates(IList<Tuple<int, string>> sources,
            IDbCommand command,
            string field,
            CancellationToken cancel,
            IProgress<int> progress = null)
        {
            int n = 0;
            foreach (var s in sources)
            {
                HistoricalDate date = _adapter.GetDate(s.Item2);
                if (date == null) continue;

                ((DbParameter)command.Parameters["@field"]).Value = field;
                ((DbParameter)command.Parameters["@targetid"]).Value = s.Item1;
                ((DbParameter)command.Parameters["@datetxt"]).Value = date.ToString();
                ((DbParameter)command.Parameters["@dateval"]).Value = date.GetSortValue();
                ((DbParameter)command.Parameters["@source"]).Value = s.Item2;
                command.ExecuteNonQuery();

                if (cancel.IsCancellationRequested) break;
                if (++n % 100 == 0) progress?.Report(n);
            }
            progress?.Report(n);
        }

        public void Index(CancellationToken cancel,
            IProgress<int> progress = null)
        {
            using IDbConnection connection = new MySqlConnection(_connString);
            connection.Open();

            // create or truncate date
            InitTarget(connection);

            // insert data from authors and works
            const string parameters = "@field,@targetid,@datetxt,@dateval,@source";
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO `date`" +
                "(`field`,`targetid`,`datetxt`,`dateval`,`source`) " +
                $"VALUES({parameters});";
            foreach (string p in parameters.Split(','))
                AddParameter(p, cmd, p == "@dateval" ? DbType.Double : DbType.String);

            var sources = CollectSources(connection, "auteurs");
            WriteDates(sources, cmd, "aut", cancel, progress);

            sources = CollectSources(connection, "oeuvres");
            WriteDates(sources, cmd, "wrk", cancel, progress);
        }
    }
}
