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
                    );
                    CREATE INDEX IF NOT EXISTS IX_Orders_OrderDate ON Orders(OrderDate);
                    CREATE INDEX IF NOT EXISTS IX_OrderLines_OrderId ON OrderLines(OrderId);");
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

        /// <inheritdoc/>
        /// <remarks>
        /// All filtering is performed in SQL.  <paramref name="text"/> is matched
        /// case-insensitively against CustomerName, Region and PaymentMethod via
        /// SQL <c>LIKE</c>.  Date bounds are inclusive on both ends.
        /// </remarks>
        public List<OrderRecord> Search(string text, DateTime? from, DateTime? to)
        {
            bool hasText = !string.IsNullOrWhiteSpace(text);
            bool hasFrom = from.HasValue;
            bool hasTo   = to.HasValue;

            var conditions = new System.Collections.Generic.List<string>();
            if (hasText) conditions.Add(
                "(LOWER(CustomerName) LIKE @Pattern OR LOWER(Region) LIKE @Pattern " +
                "OR LOWER(PaymentMethod) LIKE @Pattern OR OrderDate LIKE @Pattern)");
            if (hasFrom) conditions.Add("OrderDate >= @From");
            if (hasTo)   conditions.Add("OrderDate <= @To");

            string where  = conditions.Count > 0
                ? "WHERE " + string.Join(" AND ", conditions)
                : string.Empty;
            string sql    = $"SELECT * FROM Orders {where} ORDER BY OrderDate";
            string pattern = "%" + (text ?? string.Empty).ToLowerInvariant() + "%";

            using (var conn = OpenConnection())
            {
                var orders = conn.Query<OrderRecord>(sql, new
                {
                    Pattern = pattern,
                    From    = hasFrom ? from.Value.ToString("yyyy-MM-dd") : (object)null,
                    To      = hasTo   ? to.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)null,
                }).ToList();

                if (orders.Count == 0) return orders;

                // Load only the lines that belong to the matched orders
                var ids = string.Join(",", orders.Select(o => "'" + o.Id.Replace("'", "''") + "'"));
                var lines = conn.Query<LineRow>(
                    $"SELECT OrderId, Item, Quantity, Price FROM OrderLines WHERE OrderId IN ({ids})").ToList();

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

                    order.Subtotal = Math.Round(order.Subtotal, 2);
                    order.Tax      = Math.Round(order.Tax,      2);
                    order.Total    = Math.Round(order.Total,    2);
                }

                return orders;
            }
        }

        /// <inheritdoc/>
        public OrderSummary GetSummary()
        {
            using (var conn = OpenConnection())
            {
                var row = conn.QueryFirstOrDefault<SummaryRow>(
                    "SELECT COUNT(*) AS TotalOrders, " +
                    "       COALESCE(SUM(Total), 0) AS TotalRevenue, " +
                    "       COALESCE(AVG(Total), 0) AS AverageOrderValue " +
                    "FROM Orders");

                if (row == null)
                    return new OrderSummary();

                return new OrderSummary
                {
                    TotalOrders        = (int)row.TotalOrders,
                    TotalRevenue       = Math.Round((decimal)row.TotalRevenue,       2),
                    AverageOrderValue  = Math.Round((decimal)row.AverageOrderValue,  2),
                };
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Runs inside a transaction so the Orders row and its OrderLines are removed atomically.
        /// ON DELETE CASCADE handles the OrderLines rows automatically.
        /// </remarks>
        public void Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            using (var conn = OpenConnection())
            using (var tx = conn.BeginTransaction())
            {
                conn.Execute("DELETE FROM Orders WHERE Id = @Id", new { Id = id }, tx);
                tx.Commit();
            }
        }

        /// <inheritdoc/>
        public void VoidOrder(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            using (var conn = OpenConnection())
                conn.Execute(
                    "UPDATE Orders SET Status = 'Voided' WHERE Id = @Id",
                    new { Id = id });
        }

        /// <inheritdoc/>
        public OrderSummary GetSummaryForPeriod(DateTime? from, DateTime? to)
        {
            string where = BuildDateWhere(from, to, activeOnly: true);
            using (var conn = OpenConnection())
            {
                var row = conn.QueryFirstOrDefault<SummaryRow>(
                    $"SELECT COUNT(*) AS TotalOrders, " +
                    $"       COALESCE(SUM(Total), 0) AS TotalRevenue, " +
                    $"       COALESCE(AVG(Total), 0) AS AverageOrderValue " +
                    $"FROM Orders {where}",
                    new
                    {
                        From = from.HasValue ? from.Value.ToString("yyyy-MM-dd") : (object)null,
                        To   = to.HasValue   ? to.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)null,
                    });
                if (row == null) return new OrderSummary();
                return new OrderSummary
                {
                    TotalOrders       = (int)row.TotalOrders,
                    TotalRevenue      = Math.Round((decimal)row.TotalRevenue,      2),
                    AverageOrderValue = Math.Round((decimal)row.AverageOrderValue, 2),
                };
            }
        }

        /// <inheritdoc/>
        public List<DailySummary> GetDailySummaries(DateTime from, DateTime to)
        {
            using (var conn = OpenConnection())
                return conn.Query<DailySummaryRow>(
                    "SELECT DATE(OrderDate) AS Day, COUNT(*) AS OrderCount, " +
                    "       COALESCE(SUM(Total), 0) AS Revenue, " +
                    "       COALESCE(SUM(Tax), 0) AS Gst " +
                    "FROM Orders " +
                    "WHERE OrderDate >= @From AND OrderDate < @To " +
                    "  AND (Status IS NULL OR Status = 'Active') " +
                    "GROUP BY DATE(OrderDate) ORDER BY Day",
                    new
                    {
                        From = from.ToString("yyyy-MM-dd"),
                        To   = to.AddDays(1).ToString("yyyy-MM-dd"),
                    })
                    .Select(r => new DailySummary
                    {
                        Day        = DateTime.Parse(r.Day),
                        OrderCount = (int)r.OrderCount,
                        Revenue    = Math.Round((decimal)r.Revenue, 2),
                        Gst        = Math.Round((decimal)r.Gst,     2),
                    })
                    .ToList();
        }

        /// <inheritdoc/>
        public List<TopItem> GetTopItems(DateTime? from, DateTime? to, int limit)
        {
            string dateFilter = from.HasValue || to.HasValue
                ? "WHERE o.OrderDate >= @From AND o.OrderDate < @To " +
                  "  AND (o.Status IS NULL OR o.Status = 'Active') "
                : "WHERE (o.Status IS NULL OR o.Status = 'Active') ";
            using (var conn = OpenConnection())
                return conn.Query<TopItemRow>(
                    "SELECT ol.Item, " +
                    "       CAST(SUM(CASE WHEN ol.Quantity > 0 THEN ol.Quantity ELSE 1 END) AS INTEGER) AS TotalQty, " +
                    "       COALESCE(SUM(ol.Price), 0) AS TotalRevenue " +
                    "FROM OrderLines ol " +
                    "JOIN Orders o ON o.Id = ol.OrderId " +
                    dateFilter +
                    "  AND ol.Item NOT LIKE '  %' " +
                    "GROUP BY ol.Item " +
                    "ORDER BY TotalRevenue DESC " +
                    "LIMIT @Limit",
                    new
                    {
                        From  = from.HasValue ? from.Value.ToString("yyyy-MM-dd")  : (object)null,
                        To    = to.HasValue   ? to.Value.AddDays(1).ToString("yyyy-MM-dd") : (object)null,
                        Limit = limit,
                    })
                    .Select(r => new TopItem
                    {
                        Item         = r.Item,
                        TotalQty     = (int)r.TotalQty,
                        TotalRevenue = Math.Round((decimal)r.TotalRevenue, 2),
                    })
                    .ToList();
        }

        /// <inheritdoc/>
        public List<PaymentSplit> GetPaymentBreakdown(DateTime? from, DateTime? to)
        {
            string where = BuildDateWhere(from, to, activeOnly: true);
            using (var conn = OpenConnection())
                return conn.Query<PaymentSplitRow>(
                    "SELECT PaymentMethod, COUNT(*) AS OrderCount, " +
                    "       COALESCE(SUM(Total), 0) AS Revenue " +
                    $"FROM Orders {where} " +
                    "GROUP BY PaymentMethod ORDER BY Revenue DESC",
                    new
                    {
                        From = from.HasValue ? from.Value.ToString("yyyy-MM-dd")              : (object)null,
                        To   = to.HasValue   ? to.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)null,
                    })
                    .Select(r => new PaymentSplit
                    {
                        PaymentMethod = r.PaymentMethod,
                        OrderCount    = (int)r.OrderCount,
                        Revenue       = Math.Round((decimal)r.Revenue, 2),
                    })
                    .ToList();
        }

        private static string BuildDateWhere(DateTime? from, DateTime? to, bool activeOnly)
        {
            var parts = new List<string>();
            if (activeOnly) parts.Add("(Status IS NULL OR Status = 'Active')");
            if (from.HasValue) parts.Add("OrderDate >= @From");
            if (to.HasValue)   parts.Add("OrderDate <= @To");
            return parts.Count > 0 ? "WHERE " + string.Join(" AND ", parts) : string.Empty;
        }

        // ── Private DTOs for report queries ───────────────────────────────────

        private sealed class DailySummaryRow
        {
            public string Day        { get; set; }
            public long   OrderCount { get; set; }
            public double Revenue    { get; set; }
            public double Gst        { get; set; }
        }

        private sealed class TopItemRow
        {
            public string Item         { get; set; }
            public long   TotalQty     { get; set; }
            public double TotalRevenue { get; set; }
        }

        private sealed class PaymentSplitRow
        {
            public string PaymentMethod { get; set; }
            public long   OrderCount    { get; set; }
            public double Revenue       { get; set; }
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

        /// <summary>Row DTO for the aggregate stats query in <see cref="GetSummary"/>.</summary>
        private sealed class SummaryRow
        {
            public long   TotalOrders       { get; set; }
            public double TotalRevenue      { get; set; }
            public double AverageOrderValue { get; set; }
        }
    }
}
