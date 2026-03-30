using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Dapper;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// SQLite-backed implementation of <see cref="ISettingsRepository"/>.
    /// Reads and writes the <c>Settings</c> table created by migration
    /// <c>0001_AddSettingsTable</c>.
    /// </summary>
    public class SettingsRepository : ISettingsRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initialises using the same data directory as <see cref="OrderRepository"/>.
        /// </summary>
        public SettingsRepository(string dataDirectory)
        {
            string dbPath    = Path.Combine(dataDirectory, "orders.db");
            _connectionString = $"Data Source={dbPath};Version=3;DateTimeFormat=ISO8601;";
        }

        /// <inheritdoc/>
        public string Get(string key, string defaultValue = null)
        {
            using (var conn = Open())
            {
                string val = conn.QueryFirstOrDefault<string>(
                    "SELECT Value FROM Settings WHERE Key = @Key", new { Key = key });
                return val ?? defaultValue;
            }
        }

        /// <inheritdoc/>
        public void Set(string key, string value)
        {
            using (var conn = Open())
                conn.Execute(
                    "INSERT OR REPLACE INTO Settings (Key, Value) VALUES (@Key, @Value)",
                    new { Key = key, Value = value });
        }

        /// <inheritdoc/>
        public IReadOnlyList<SettingRow> GetAll()
        {
            using (var conn = Open())
                return conn.Query<SettingRow>(
                    "SELECT Key, Value FROM Settings ORDER BY Key")
                    .ToList();
        }

        private SQLiteConnection Open()
        {
            var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
