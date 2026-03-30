using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Dapper;

namespace WindowsFormsApplication3.Infrastructure
{
    /// <summary>
    /// Lightweight database migration runner backed by SQLite + Dapper.
    /// Safe to call on every startup.
    /// </summary>
    public static class DatabaseMigrator
    {
        private const string TrackingTable = "SchemaHistory";

        /// <summary>
        /// Ensures the target database exists and all pending migrations are applied.
        /// </summary>
        public static void Run(string dataDirectory)
        {
            Directory.CreateDirectory(dataDirectory);
            string dbPath = Path.Combine(dataDirectory, "orders.db");
            string cs = $"Data Source={dbPath};Version=3;DateTimeFormat=ISO8601;";

            using (var conn = new SQLiteConnection(cs))
            {
                conn.Open();
                conn.Execute("PRAGMA foreign_keys = ON;");
                EnsureTrackingTable(conn);
                EnsureCoreTables(conn);

                foreach (var migration in Migrations)
                    ApplyIfNotRun(conn, migration);
            }
        }

        private static void EnsureTrackingTable(IDbConnection conn)
        {
            conn.Execute(@"
                CREATE TABLE IF NOT EXISTS SchemaHistory (
                    Script    TEXT PRIMARY KEY NOT NULL,
                    AppliedAt TEXT NOT NULL
                )");
        }

        private static void EnsureCoreTables(IDbConnection conn)
        {
            conn.Execute(@"
                CREATE TABLE IF NOT EXISTS Orders (
                    Id                  TEXT PRIMARY KEY NOT NULL,
                    OrderDate           TEXT NOT NULL,
                    CustomerName        TEXT,
                    Address             TEXT,
                    City                TEXT,
                    Region              TEXT,
                    PostalCode          TEXT,
                    PaymentMethod       TEXT,
                    Subtotal            REAL NOT NULL DEFAULT 0,
                    Tax                 REAL NOT NULL DEFAULT 0,
                    Total               REAL NOT NULL DEFAULT 0,
                    Status              TEXT NOT NULL DEFAULT 'Active',
                    Discount            REAL NOT NULL DEFAULT 0,
                    DiscountDescription TEXT
                );
                CREATE TABLE IF NOT EXISTS OrderLines (
                    RowId    INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderId  TEXT NOT NULL REFERENCES Orders(Id) ON DELETE CASCADE,
                    Item     TEXT,
                    Quantity INTEGER NOT NULL DEFAULT 0,
                    Price    REAL NOT NULL DEFAULT 0
                );
                CREATE TABLE IF NOT EXISTS Settings (
                    Key   TEXT PRIMARY KEY NOT NULL,
                    Value TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_Orders_OrderDate ON Orders(OrderDate);
                CREATE INDEX IF NOT EXISTS IX_OrderLines_OrderId ON OrderLines(OrderId);");

            SeedDefaultSettings(conn);
        }

        private static void SeedDefaultSettings(IDbConnection conn)
        {
            conn.Execute(@"
                INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('PizzaPrice.Small',      '4.00');
                INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('PizzaPrice.Medium',     '7.00');
                INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('PizzaPrice.Large',      '10.00');
                INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('PizzaPrice.ExtraLarge', '13.00');
                INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('ToppingPrice',          '0.75');
                INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('DrinkCanPrice',         '1.45');
                INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('WaterPrice',            '1.25');
                INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('SidePrice',             '3.00');
                INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('DeliveryMinutes',       '30');");
        }

        private static void ApplyIfNotRun(IDbConnection conn, Migration migration)
        {
            bool already = conn.QueryFirstOrDefault<int>(
                $"SELECT COUNT(*) FROM {TrackingTable} WHERE Script = @Script",
                new { Script = migration.Name }) > 0;

            if (already)
                return;

            migration.Apply(conn);
            conn.Execute(
                $"INSERT INTO {TrackingTable} (Script, AppliedAt) VALUES (@Script, @AppliedAt)",
                new { Script = migration.Name, AppliedAt = DateTime.UtcNow.ToString("o") });
        }

        private static void EnsureColumnExists(IDbConnection conn, string tableName, string columnName, string alterSql)
        {
            bool exists = conn.Query<TableInfoRow>($"PRAGMA table_info({tableName})")
                .Any(row => string.Equals(row.name, columnName, StringComparison.OrdinalIgnoreCase));

            if (!exists)
                conn.Execute(alterSql);
        }

        private static readonly IReadOnlyList<Migration> Migrations =
            new List<Migration>
            {
                new Migration("0001_AddSettingsTable", SeedDefaultSettings),
                new Migration("0002_AddOrderStatusAndPin", conn =>
                {
                    EnsureColumnExists(
                        conn,
                        "Orders",
                        "Status",
                        "ALTER TABLE Orders ADD COLUMN Status TEXT NOT NULL DEFAULT 'Active';");
                    conn.Execute("INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('StaffPin', '');");
                }),
                new Migration("0003_AddOrderDiscountFields", conn =>
                {
                    EnsureColumnExists(
                        conn,
                        "Orders",
                        "Discount",
                        "ALTER TABLE Orders ADD COLUMN Discount REAL NOT NULL DEFAULT 0;");
                    EnsureColumnExists(
                        conn,
                        "Orders",
                        "DiscountDescription",
                        "ALTER TABLE Orders ADD COLUMN DiscountDescription TEXT;");
                }),
            };

        private sealed class Migration
        {
            public Migration(string name, Action<IDbConnection> apply)
            {
                Name = name;
                Apply = apply;
            }

            public string Name { get; private set; }
            public Action<IDbConnection> Apply { get; private set; }
        }

        private sealed class TableInfoRow
        {
            public string name { get; set; }
        }
    }
}
