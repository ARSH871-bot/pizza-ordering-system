using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using Dapper;
using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Persists and retrieves <see cref="OrderRecord"/> instances using an embedded
    /// SQLite database at <c>%APPDATA%\PizzaExpress\orders.db</c>.
    /// <para>
    /// The schema is created automatically on first run. Existing NDJSON
    /// (<c>orders.ndjson</c>) or legacy JSON-array (<c>orders.json</c>) data is
    /// migrated transparently on first launch — no manual steps required.
    /// </para>
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly string _dbPath;

        private string ConnectionString
            => $"Data Source={_dbPath};Version=3;DateTimeFormat=ISO8601;";

        /// <summary>Initialises using the default AppData directory.</summary>
        public OrderRepository()
            : this(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PizzaExpress"))
        {
        }

        /// <summary>
        /// Initialises using a custom <paramref name="dataDirectory"/>.
        /// Primarily used by unit tests to avoid touching real user data.
        /// </summary>
        public OrderRepository(string dataDirectory)
        {
            Directory.CreateDirectory(dataDirectory);
            _dbPath = Path.Combine(dataDirectory, "orders.db");
            InitialiseSchema();
            TryMigrateFromLegacy(dataDirectory);
        }

        // ── Schema ────────────────────────────────────────────────────────────

        private IDbConnection OpenConnection()
        {
            var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        private void InitialiseSchema()
        {
            using (var conn = OpenConnection())
            {
                conn.Execute(@"
                    CREATE TABLE IF NOT EXISTS Orders (
                        Id            TEXT PRIMARY KEY NOT NULL,
                        OrderDate     TEXT NOT NULL,
                        CustomerName  TEXT,
                        Address       TEXT,
                        City          TEXT,
                        Region        TEXT,
                        PostalCode    TEXT,
                        PaymentMethod TEXT,
                        Subtotal      REAL NOT NULL DEFAULT 0,
                        Tax           REAL NOT NULL DEFAULT 0,
                        Total         REAL NOT NULL DEFAULT 0
                    );
                    CREATE TABLE IF NOT EXISTS OrderLines (
                        RowId    INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrderId  TEXT NOT NULL REFERENCES Orders(Id) ON DELETE CASCADE,
                        Item     TEXT,
                        Quantity INTEGER NOT NULL DEFAULT 0,
                        Price    REAL    NOT NULL DEFAULT 0
                    );");
            }
        }

        // ── IOrderRepository ──────────────────────────────────────────────────

        /// <inheritdoc/>
        /// <remarks>Runs inside a transaction — both the order header and all
        /// line items are written atomically.</remarks>
        public void Save(OrderRecord record)
        {
            if (record == null) throw new ArgumentNullException("record");

            using (var conn = OpenConnection())
            using (var tx = conn.BeginTransaction())
            {
                conn.Execute(@"
                    INSERT OR REPLACE INTO Orders
                        (Id, OrderDate, CustomerName, Address, City, Region,
                         PostalCode, PaymentMethod, Subtotal, Tax, Total)
                    VALUES
                        (@Id, @OrderDate, @CustomerName, @Address, @City, @Region,
                         @PostalCode, @PaymentMethod, @Subtotal, @Tax, @Total)",
                    record, tx);

                if (record.Lines.Count > 0)
                {
                    conn.Execute(
                        "INSERT INTO OrderLines (OrderId, Item, Quantity, Price) " +
                        "VALUES (@OrderId, @Item, @Quantity, @Price)",
                        record.Lines.Select(l =>
                            new { OrderId = record.Id, l.Item, l.Quantity, l.Price }),
                        tx);
                }

                tx.Commit();
            }
        }

        /// <inheritdoc/>
        /// <remarks>Orders are returned oldest-first (ascending <c>OrderDate</c>).
        /// Financial fields are rounded to 2 decimal places after loading to
        /// eliminate any floating-point artefacts from SQLite REAL storage.</remarks>
        public List<OrderRecord> LoadAll()
        {
            using (var conn = OpenConnection())
            {
                var orders = conn.Query<OrderRecord>(
                    "SELECT * FROM Orders ORDER BY OrderDate").ToList();

                if (orders.Count == 0) return orders;

                var lines = conn.Query<LineRow>(
                    "SELECT OrderId, Item, Quantity, Price FROM OrderLines").ToList();

                var grouped = lines
                    .GroupBy(l => l.OrderId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(l => new OrderLineRecord
                        {
                            Item     = l.Item,
                            Quantity = (int)l.Quantity,
                            Price    = Math.Round((decimal)l.Price, 2),
                        }).ToList());

                foreach (var order in orders)
                {
                    List<OrderLineRecord> orderLines;
                    if (grouped.TryGetValue(order.Id, out orderLines))
                        order.Lines = orderLines;

                    // Round to 2 d.p. — eliminates double→decimal artefacts
                    order.Subtotal = Math.Round(order.Subtotal, 2);
                    order.Tax      = Math.Round(order.Tax,      2);
                    order.Total    = Math.Round(order.Total,    2);
                }

                return orders;
            }
        }

        // ── Legacy migration ──────────────────────────────────────────────────

        private void TryMigrateFromLegacy(string dataDirectory)
        {
            // Priority 1 — NDJSON (v2.4.0–v2.6.0 format): one JSON object per line
            string ndjsonPath = Path.Combine(dataDirectory, "orders.ndjson");
            if (File.Exists(ndjsonPath))
            {
                MigrateFromNdjson(ndjsonPath);
                return;
            }

            // Priority 2 — JSON array (pre-v2.4.0 format): entire history in one array
            string jsonPath = Path.Combine(dataDirectory, "orders.json");
            if (File.Exists(jsonPath))
                MigrateFromJsonArray(jsonPath);
        }

        private void MigrateFromNdjson(string ndjsonPath)
        {
            try
            {
                var serialiser = new JavaScriptSerializer();
                foreach (string line in File.ReadLines(ndjsonPath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    try
                    {
                        var record = serialiser.Deserialize<OrderRecord>(line);
                        if (record != null) Save(record);
                    }
                    catch { /* skip corrupted lines */ }
                }

                File.Move(ndjsonPath, ndjsonPath + ".migrated");
            }
            catch { /* migration failure is non-fatal */ }
        }

        private void MigrateFromJsonArray(string jsonPath)
        {
            try
            {
                var serialiser = new JavaScriptSerializer();
                string json    = File.ReadAllText(jsonPath);
                var    records = serialiser.Deserialize<List<OrderRecord>>(json)
                                 ?? new List<OrderRecord>();

                foreach (var r in records)
                    Save(r);

                File.Move(jsonPath, jsonPath + ".migrated");
            }
            catch { /* migration failure is non-fatal */ }
        }

        // ── Private DTO ───────────────────────────────────────────────────────

        /// <summary>
        /// Row DTO used when loading <see cref="OrderLines"/> from the database.
        /// Uses <c>long</c>/<c>double</c> to match SQLite's native INTEGER/REAL types.
        /// </summary>
        private sealed class LineRow
        {
            public string OrderId  { get; set; }
            public string Item     { get; set; }
            public long   Quantity { get; set; }
            public double Price    { get; set; }
        }
    }
}
