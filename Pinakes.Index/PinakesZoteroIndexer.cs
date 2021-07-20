using Fusi.Tools;
using Microsoft.Extensions.Logging;
using Pinakes.Zotero;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;

namespace Pinakes.Index
{
    /// <summary>
    /// Pinakes Zotero indexer.
    /// </summary>
    /// <seealso cref="PinakesIndexer" />
    public sealed class PinakesZoteroIndexer : PinakesIndexer
    {
        private ZoteroClient _client;

        /// <summary>
        /// Gets or sets the Zotero API key to use.
        /// </summary>
        public string ZoteroKey { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PinakesZoteroIndexer"/>
        /// class.
        /// </summary>
        /// <param name="connString">The connection string.</param>
        public PinakesZoteroIndexer(string connString) : base(connString)
        {
        }

        private static void InitTarget(IDbConnection connection)
        {
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = LoadResourceText("Zotero.mysql");
            cmd.ExecuteNonQuery();

            cmd.CommandText = "TRUNCATE TABLE pix_zotero;";
            cmd.ExecuteNonQuery();
        }

        private static IList<Tuple<int,string>> GetAuthorZoteroIds(
            IDbConnection connection)
        {
            using IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT DISTINCT a.id, m.cle_zotero FROM auteurs a " +
                "INNER JOIN oeuvres_auteurs wa ON a.id=wa.id_auteur " +
                "INNER JOIN identifiants_oeuvres wi ON wi.id_oeuvre=wa.id_oeuvre " +
                "INNER JOIN identifiants i ON wi.id_identifiant=i.id " +
                "INNER JOIN mobigen_auteurs ma ON a.id=ma.id_auteur " +
                "INNER JOIN mobigen m ON ma.id_mobigen=m.id " +
                "WHERE i.id_type=234;";
            List<Tuple<int, string>> ids = new List<Tuple<int,string>>();
            using IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                ids.Add(Tuple.Create(reader.GetInt32(0), reader.GetString(1)));
            return ids;
        }

        private static IList<Tuple<int, string>> GetWorkZoteroIds(
            IDbConnection connection)
        {
            using IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT w.id, m.cle_zotero FROM oeuvres w " +
                "INNER JOIN identifiants_oeuvres wi ON w.id=wi.id_oeuvre " +
                "INNER JOIN identifiants i ON wi.id_identifiant=i.id " +
                "INNER JOIN mobigen_oeuvres mw ON w.id=mw.id_oeuvre " +
                "INNER JOIN mobigen m ON mw.id_mobigen=m.id " +
                "WHERE i.id_type=234;";
            List<Tuple<int, string>> ids = new List<Tuple<int, string>>();
            using IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                ids.Add(Tuple.Create(reader.GetInt32(0), reader.GetString(1)));
            return ids;
        }

        private void WriteIndexes(IDbConnection connection,
            IDbCommand command,
            bool forAuthor,
            CancellationToken cancel,
            IProgress<ProgressReport> progress = null)
        {
            ProgressReport report = progress != null ? new ProgressReport() : null;
            progress?.Report(report);
            IList<Tuple<int, string>> ids = forAuthor
                ? GetAuthorZoteroIds(connection)
                : GetWorkZoteroIds(connection);

            foreach (var t in ids)
            {
                BiblioItemFragment fr = _client.GetItemFragment(
                    t.Item2, t.Item1, forAuthor);
                if (fr == null)
                {
                    Logger?.LogError($"Zotero item with ID {t.Item2} not found");
                    continue;
                }
                ((DbParameter)command.Parameters["@zotero_id"]).Value = fr.ZoteroId;
                ((DbParameter)command.Parameters["@author_target"]).Value =
                    fr.IsAuthorTarget;
                ((DbParameter)command.Parameters["@target_id"]).Value = fr.TargetId;
                ((DbParameter)command.Parameters["@authors"]).Value = fr.Authors;
                ((DbParameter)command.Parameters["@title"]).Value = fr.Title;
                ((DbParameter)command.Parameters["@abstract"]).Value = fr.Abstract;

                command.ExecuteNonQuery();
                if (cancel.IsCancellationRequested) break;
                if (progress != null && ++report.Count % 10 ==0)
                {
                    report.Percent = report.Count * 100 / ids.Count;
                    progress.Report(report);
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
            CancellationToken cancel, IProgress<ProgressReport> progress = null)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (string.IsNullOrEmpty(ZoteroKey))
                throw new InvalidOperationException("Zotero key not set");

            _client = new ZoteroClient(ZoteroKey);

            // create or truncate date
            InitTarget(connection);

            // prepare insert command
            IDbCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO `pix_zotero`\n" +
                "(`zotero_id`, `author_target`, `target_id`, `authors`, `title`, " +
                "`abstract`)\n" +
                "VALUES" +
                "(@zotero_id, @author_target, @target_id, @authors, @title, @abstract);";
            AddParameter("@zotero_id", command, DbType.String);
            AddParameter("@author_target", command, DbType.Boolean);
            AddParameter("@target_id", command, DbType.Int32);
            AddParameter("@authors", command, DbType.String);
            AddParameter("@title", command, DbType.String);
            AddParameter("@abstract", command, DbType.String);

            // authors
            WriteIndexes(connection, command, true, cancel, progress);

            if (cancel.IsCancellationRequested) return;

            // works
            WriteIndexes(connection, command, false, cancel, progress);
        }
    }
}
