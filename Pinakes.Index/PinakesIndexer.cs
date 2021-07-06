using Fusi.Tools;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Pinakes.Index
{
    /// <summary>
    /// Base class for additional Pinakes indexers.
    /// </summary>
    public abstract class PinakesIndexer
    {
        private readonly string _connString;

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PinakesIndexer"/> class.
        /// </summary>
        /// <param name="connString">The connection string.</param>
        /// <exception cref="ArgumentNullException">connString</exception>
        protected PinakesIndexer(string connString)
        {
            _connString = connString
                ?? throw new ArgumentNullException(nameof(connString));
        }

        /// <summary>
        /// Loads a text from the specified embedded resource.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Text.</returns>
        /// <exception cref="ArgumentNullException">name</exception>
        protected static string LoadResourceText(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            using (StreamReader reader = new StreamReader(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    $"Pinakes.Index.Assets.{name}"), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <returns>Connection.</returns>
        protected IDbConnection GetConnection() => new MySqlConnection(_connString);

        /// <summary>
        /// Adds the specified parameter to <paramref name="command"/>.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="command">The target command.</param>
        /// <param name="type">The parameter type.</param>
        /// <exception cref="ArgumentNullException">name or command</exception>
        protected static void AddParameter(string name, IDbCommand command,
            DbType type)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (command == null) throw new ArgumentNullException(nameof(command));

            var p = command.CreateParameter();
            p.DbType = type;
            p.ParameterName = name;
            p.Direction = ParameterDirection.Input;
            command.Parameters.Add(p);
        }

        /// <summary>
        /// Perform the indexing.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="cancel">The cancel token.</param>
        /// <param name="progress">The optional progress reporter.</param>
        protected abstract void DoIndex(IDbConnection connection,
            CancellationToken cancel,
            IProgress<ProgressReport> progress = null);

        /// <summary>
        /// Index the required resources in the Pinakes database.
        /// </summary>
        /// <param name="cancel">The cancel token.</param>
        /// <param name="progress">The optional progress reporter.</param>
        public void Index(CancellationToken cancel,
            IProgress<ProgressReport> progress = null)
        {
            using IDbConnection connection = new MySqlConnection(_connString);
            connection.Open();

            DoIndex(connection, cancel, progress);
        }
    }
}
