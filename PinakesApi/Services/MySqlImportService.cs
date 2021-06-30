using MySql.Data.MySqlClient;
using MySQLBackupNetCore;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace PinakesApi.Services
{
    // https://www.codeproject.com/Articles/256466/MySqlBackup-NET#aspnet

    /// <summary>
    /// Import service for MySql databases.
    /// </summary>
    public sealed class MySqlImportService
    {
        /// <summary>
        /// Imports a database from the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The connection string to the target database.</param>
        /// <exception cref="ArgumentNullException">source or target</exception>
        /// <exception cref="InvalidDataException">Invalid source</exception>
        public void Import(string source, string target)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            using (FileStream fs = new FileStream(source, FileMode.Open,
                FileAccess.Read))
            using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read))
            using (MySqlConnection connection = new MySqlConnection(target))
            {
                ZipArchiveEntry entry = zip.Entries.FirstOrDefault(e =>
                    e.Name.EndsWith(".sql",
                    StringComparison.InvariantCultureIgnoreCase));
                if (entry == null)
                {
                    throw new InvalidDataException(
                        $"No .sql entry in archive {source}");
                }

                connection.Open();
                MySqlCommand cmd = connection.CreateCommand();
                MySqlBackup backup = new MySqlBackup(cmd);
                using (Stream stream = entry.Open())
                {
                    Console.Write($"Importing from {source}...");
                    backup.ImportFromStream(stream);
                    Console.WriteLine(" complete.");
                }
            }
        }
    }
}
