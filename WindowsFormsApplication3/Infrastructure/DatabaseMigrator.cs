using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Dapper;

namespace WindowsFormsApplication3.Infrastructure
{
    /// <summary>
    /// Lightweight database migration runner backed by SQLite + Dapper.
    /// <para>
    /// Each migration script is stored inline as a constant string and identified by a
    /// unique name.  Applied scripts are recorded in a <c>SchemaHistory</c> table so
    /// each script runs exactly once, in declaration order, across all environments and
    /// upgrade paths — no DbUp or FluentMigrator dependency required.
    /// </para>
    /// </summary>
    public static class DatabaseMigrator
    {
        private const string TrackingTable = "SchemaHistory";

        /// <summary>
        /// Ensures the target database is up-to-date by applying all pending migrations.
        /// Safe to call on every application start — already-applied scripts are skipped.
        /// </summary>
        /// <param name="dataDirectory">
        /// Directory that contains (or will contain) <c>orders.db</c>.
        /// </param>
        public static void Run(string dataDirectory)
        {
            Directory.CreateDirectory(dataDirectory);
            string dbPath = Path.Combine(dataDirectory, "orders.db");
            string cs     = $"Data Source={dbPath};Version=3;DateTimeFormat=ISO8601;";

            using (var conn = new SQLiteConnection(cs))
            {
                conn.Open();
                EnsureTrackingTable(conn);

                foreach (var migration in Migrations)
                    ApplyIfNotRun(conn, migration.Key, migration.Value);
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static void EnsureTrackingTable(IDbConnection conn)
        {
            conn.Execute(@"
                CREATE TABLE IF NOT EXISTS SchemaHistory (
                    Script    TEXT PRIMARY KEY NOT NULL,
                    AppliedAt TEXT NOT NULL
                )");
        }

        private static void ApplyIfNotRun(IDbConnection conn, string name, string sql)
        {
            bool already = conn.QueryFirstOrDefault<int>(
                "SELECT COUNT(*) FROM SchemaHistory WHERE Script = @Script",
                new { Script = name }) > 0;

            if (already) return;

            conn.Execute(sql);
            conn.Execute(
                "INSERT INTO SchemaHistory (Script, AppliedAt) VALUES (@Script, @AppliedAt)",
                new { Script = name, AppliedAt = DateTime.UtcNow.ToString("o") });
        }

        // ── Migration scripts — add new entries at the bottom only ────────────

        private static readonly IReadOnlyList<KeyValuePair<string, string>> Migrations =
            new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("0001_AddSettingsTable",       Script0001),
                new KeyValuePair<string, string>("0002_AddOrderStatusAndPin",   Script0002),
            };

        private const string Script0002 = @"
            ALTER TABLE Orders ADD COLUMN Status TEXT NOT NULL DEFAULT 'Active';
            INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('StaffPin', '');
        ";

        private const string Script0001 = @"
            CREATE TABLE IF NOT EXISTS Settings (
                Key   TEXT PRIMARY KEY NOT NULL,
                Value TEXT NOT NULL
            );
            INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('PizzaPrice.Small',      '4.00');
            INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('PizzaPrice.Medium',     '7.00');
            INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('PizzaPrice.Large',      '10.00');
            INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('PizzaPrice.ExtraLarge', '13.00');
            INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('ToppingPrice',          '0.75');
            INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('DrinkCanPrice',         '1.45');
            INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('WaterPrice',            '1.25');
            INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('SidePrice',             '3.00');
            INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('DeliveryMinutes',       '30');
        ";
    }
}
