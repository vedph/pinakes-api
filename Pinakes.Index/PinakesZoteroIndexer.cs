using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pinakes.Index
{
    /// <summary>
    /// Pinakes Zotero indexer.
    /// </summary>
    /// <seealso cref="PinakesIndexer" />
    public sealed class PinakesZoteroIndexer : PinakesIndexer
    {
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

            cmd.CommandText = "TRUNCATE TABLE zotero;";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Perform the indexing.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="cancel">The cancel token.</param>
        /// <param name="progress">The optional progress reporter.</param>
        /// <exception cref="ArgumentNullException">connection</exception>
        protected override void DoIndex(IDbConnection connection,
            CancellationToken cancel, IProgress<int> progress = null)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            // create or truncate date
            InitTarget(connection);

            throw new NotImplementedException();
        }
    }
}
