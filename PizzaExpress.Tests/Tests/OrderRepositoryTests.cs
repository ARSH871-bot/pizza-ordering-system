using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    /// <summary>
    /// Integration tests for OrderRepository — exercises real file I/O against a temp directory.
    /// </summary>
    [TestClass]
    public class OrderRepositoryTests
    {
        private string _tempDir;

        [TestInitialize]
        public void SetUp()
        {
            // Fresh isolated temp directory per test — never touches real user data
            _tempDir = Path.Combine(Path.GetTempPath(), "PizzaExpressTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TestCleanup]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        private OrderRepository MakeRepo() => new OrderRepository(_tempDir);

        // ── Save & Load ───────────────────────────────────────────────────────

        [TestMethod]
        public void Save_SingleRecord_CanBeLoadedBack()
        {
            var repo   = MakeRepo();
            var record = MakeRecord("John Smith");

            repo.Save(record);
            List<OrderRecord> loaded = repo.LoadAll();

            Assert.AreEqual(1, loaded.Count);
            Assert.AreEqual("John Smith", loaded[0].CustomerName);
        }

        [TestMethod]
        public void Save_MultipleRecords_AllPersisted()
        {
            var repo = MakeRepo();
            repo.Save(MakeRecord("Alice"));
            repo.Save(MakeRecord("Bob"));
            repo.Save(MakeRecord("Charlie"));

            List<OrderRecord> loaded = repo.LoadAll();

            Assert.AreEqual(3, loaded.Count);
        }

        [TestMethod]
        public void Save_PreservesAllFields()
        {
            var repo = MakeRepo();
            var record = new OrderRecord
            {
                Id            = "ABC123",
                OrderDate     = new DateTime(2026, 3, 24, 12, 0, 0),
                CustomerName  = "Jane Doe",
                Address       = "1 Queen St",
                City          = "Auckland",
                Region        = "Auckland",
                PostalCode    = "1010",
                PaymentMethod = "Credit Card",
                Subtotal      = 15.00m,
                Tax           = 2.25m,
                Total         = 17.25m,
            };
            record.Lines.Add(new OrderLineRecord { Item = "Normal Crust Small Pizza", Quantity = 1, Price = 4.00m });

            repo.Save(record);
            var loaded = repo.LoadAll()[0];

            Assert.AreEqual("ABC123",      loaded.Id);
            Assert.AreEqual("Jane Doe",    loaded.CustomerName);
            Assert.AreEqual("1 Queen St",  loaded.Address);
            Assert.AreEqual("Auckland",    loaded.City);
            Assert.AreEqual("Auckland",    loaded.Region);
            Assert.AreEqual("1010",        loaded.PostalCode);
            Assert.AreEqual("Credit Card", loaded.PaymentMethod);
            Assert.AreEqual(15.00m,        loaded.Subtotal);
            Assert.AreEqual(2.25m,         loaded.Tax);
            Assert.AreEqual(17.25m,        loaded.Total);
            Assert.AreEqual(1,             loaded.Lines.Count);
            Assert.AreEqual("Normal Crust Small Pizza", loaded.Lines[0].Item);
        }

        [TestMethod]
        public void LoadAll_WhenFileDoesNotExist_ReturnsEmptyList()
        {
            var repo = MakeRepo();
            List<OrderRecord> result = repo.LoadAll();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void LoadAll_WhenFileIsCorrupted_ReturnsEmptyList()
        {
            File.WriteAllText(Path.Combine(_tempDir, "orders.ndjson"), "{ not valid json [[[");

            var repo   = MakeRepo();
            var result = repo.LoadAll();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Save_CreatesDirectoryIfMissing()
        {
            // Use a sub-dir that doesn't yet exist
            string subDir = Path.Combine(_tempDir, "NewSubDir");
            var repo = new OrderRepository(subDir);
            repo.Save(MakeRecord("Test"));

            Assert.IsTrue(Directory.Exists(subDir));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Save_NullRecord_ThrowsArgumentNullException()
        {
            MakeRepo().Save(null);
        }

        [TestMethod]
        public void LoadAll_CorruptedLineSkipped_ValidLinesReturned()
        {
            // Write one valid line, one garbage line, another valid line
            string validLine = "{\"Id\":\"AAA\",\"CustomerName\":\"Alice\",\"OrderDate\":\"\\/Date(0)\\/\",\"Lines\":[]}";
            File.WriteAllText(Path.Combine(_tempDir, "orders.ndjson"),
                validLine + "\n" +
                "{ GARBAGE }\n" +
                validLine.Replace("AAA", "BBB") + "\n");

            var result = MakeRepo().LoadAll();
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void LoadAll_MigratesLegacyJsonArray()
        {
            // Write legacy JSON array file BEFORE creating the repository so the
            // constructor's migration logic picks it up on first run.
            string legacyJson = "[{\"Id\":\"LEG001\",\"CustomerName\":\"LegacyUser\"," +
                                "\"OrderDate\":\"\\/Date(0)\\/\",\"Lines\":[]}]";
            File.WriteAllText(Path.Combine(_tempDir, "orders.json"), legacyJson);

            // Construction triggers migration: orders.json → SQLite
            var result = MakeRepo().LoadAll();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("LegacyUser", result[0].CustomerName);

            // Original file renamed to prevent re-migration
            Assert.IsTrue(File.Exists(Path.Combine(_tempDir, "orders.json.migrated")));

            // SQLite database created
            Assert.IsTrue(File.Exists(Path.Combine(_tempDir, "orders.db")));
        }

        [TestMethod]
        public void LoadAll_EmptyFile_ReturnsEmptyList()
        {
            // Create the file but write nothing to it
            File.WriteAllText(Path.Combine(_tempDir, "orders.ndjson"), string.Empty);

            var result = MakeRepo().LoadAll();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Save_MultipleRecords_LoadedInChronologicalOrder()
        {
            // Use explicit dates to guarantee ORDER BY OrderDate is deterministic
            var repo = MakeRepo();
            repo.Save(MakeRecordOnDate("First",  new DateTime(2026, 1, 1)));
            repo.Save(MakeRecordOnDate("Second", new DateTime(2026, 1, 2)));
            repo.Save(MakeRecordOnDate("Third",  new DateTime(2026, 1, 3)));

            var loaded = repo.LoadAll();

            Assert.AreEqual("First",  loaded[0].CustomerName);
            Assert.AreEqual("Second", loaded[1].CustomerName);
            Assert.AreEqual("Third",  loaded[2].CustomerName);
        }

        // ── Search ───────────────────────────────────────────────────────────

        [TestMethod]
        public void Search_NullText_NoDateRange_ReturnsAllOrders()
        {
            var repo = MakeRepo();
            repo.Save(MakeRecordOnDate("Alice",   new DateTime(2026, 1, 1)));
            repo.Save(MakeRecordOnDate("Bob",     new DateTime(2026, 1, 2)));
            repo.Save(MakeRecordOnDate("Charlie", new DateTime(2026, 1, 3)));

            var result = repo.Search(null, null, null);

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void Search_ByCustomerName_ReturnsMatchingOrders()
        {
            var repo = MakeRepo();
            repo.Save(MakeRecord("Alice"));
            repo.Save(MakeRecord("Bob"));
            repo.Save(MakeRecord("Bobby"));

            var result = repo.Search("bob", null, null);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.TrueForAll(r => r.CustomerName.IndexOf("Bob", StringComparison.OrdinalIgnoreCase) >= 0));
        }

        [TestMethod]
        public void Search_ByDateRange_ReturnsOnlyOrdersInRange()
        {
            var repo = MakeRepo();
            repo.Save(MakeRecordOnDate("Jan",  new DateTime(2026, 1, 15)));
            repo.Save(MakeRecordOnDate("Feb",  new DateTime(2026, 2, 15)));
            repo.Save(MakeRecordOnDate("Mar",  new DateTime(2026, 3, 15)));

            var result = repo.Search(null, new DateTime(2026, 2, 1), new DateTime(2026, 2, 28, 23, 59, 59));

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Feb", result[0].CustomerName);
        }

        [TestMethod]
        public void Search_TextAndDateRange_ReturnsIntersection()
        {
            var repo = MakeRepo();
            repo.Save(MakeRecordOnDate("Alice", new DateTime(2026, 1, 5)));
            repo.Save(MakeRecordOnDate("Alice", new DateTime(2026, 3, 5)));
            repo.Save(MakeRecordOnDate("Bob",   new DateTime(2026, 1, 5)));

            // Alice in January only
            var result = repo.Search("Alice", new DateTime(2026, 1, 1), new DateTime(2026, 1, 31, 23, 59, 59));

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Alice", result[0].CustomerName);
            Assert.AreEqual(new DateTime(2026, 1, 5), result[0].OrderDate);
        }

        [TestMethod]
        public void Search_NoMatch_ReturnsEmptyList()
        {
            var repo = MakeRepo();
            repo.Save(MakeRecord("Alice"));

            var result = repo.Search("zzznomatch", null, null);

            Assert.AreEqual(0, result.Count);
        }

        // ── Delete ───────────────────────────────────────────────────────────

        [TestMethod]
        public void Delete_ExistingRecord_RemovesItFromStore()
        {
            var repo   = MakeRepo();
            var record = MakeRecord("ToDelete");
            repo.Save(record);

            repo.Delete(record.Id);

            Assert.AreEqual(0, repo.LoadAll().Count);
        }

        [TestMethod]
        public void Delete_ExistingRecord_RemovesItsLines()
        {
            var repo   = MakeRepo();
            var record = MakeRecord("WithLines");
            record.Lines.Add(new OrderLineRecord { Item = "Pepperoni Pizza", Quantity = 1, Price = 10.00m });
            repo.Save(record);

            repo.Delete(record.Id);
            var remaining = repo.LoadAll();

            Assert.AreEqual(0, remaining.Count);
        }

        [TestMethod]
        public void Delete_NonExistentId_DoesNotThrow()
        {
            var repo = MakeRepo();
            repo.Delete("DOES_NOT_EXIST"); // must not throw
        }

        [TestMethod]
        public void Delete_NullOrEmptyId_DoesNotThrow()
        {
            var repo = MakeRepo();
            repo.Delete(null);
            repo.Delete(string.Empty);
        }

        // ── GetSummary ────────────────────────────────────────────────────────

        [TestMethod]
        public void GetSummary_NoOrders_ReturnsZeroes()
        {
            var summary = MakeRepo().GetSummary();

            Assert.AreEqual(0,    summary.TotalOrders);
            Assert.AreEqual(0m,   summary.TotalRevenue);
            Assert.AreEqual(0m,   summary.AverageOrderValue);
        }

        [TestMethod]
        public void GetSummary_WithOrders_ReturnsCorrectAggregates()
        {
            var repo = MakeRepo();
            repo.Save(MakeRecord("A"));  // Total = 11.50
            repo.Save(MakeRecord("B"));  // Total = 11.50
            repo.Save(MakeRecord("C"));  // Total = 11.50

            var summary = repo.GetSummary();

            Assert.AreEqual(3,      summary.TotalOrders);
            Assert.AreEqual(34.50m, summary.TotalRevenue);
            Assert.AreEqual(11.50m, summary.AverageOrderValue);
        }

        // ── VoidOrder ─────────────────────────────────────────────────────────

        [TestMethod]
        public void VoidOrder_SetsStatusToVoided()
        {
            var repo   = MakeRepo();
            var record = MakeRecord("VoidMe");
            repo.Save(record);

            repo.VoidOrder(record.Id);

            var loaded = repo.LoadAll()[0];
            Assert.AreEqual("Voided", loaded.Status);
        }

        [TestMethod]
        public void VoidOrder_OrderRemainsInLoadAll()
        {
            var repo   = MakeRepo();
            var record = MakeRecord("VoidButKeep");
            repo.Save(record);

            repo.VoidOrder(record.Id);

            Assert.AreEqual(1, repo.LoadAll().Count);
        }

        [TestMethod]
        public void VoidOrder_NonExistentId_DoesNotThrow()
        {
            MakeRepo().VoidOrder("DOES_NOT_EXIST");
        }

        [TestMethod]
        public void VoidOrder_ExcludedFromGetSummaryForPeriod()
        {
            var repo = MakeRepo();
            var active = MakeRecordOnDate("Active", new DateTime(2026, 1, 10));
            var voided = MakeRecordOnDate("Voided", new DateTime(2026, 1, 10));
            repo.Save(active);
            repo.Save(voided);
            repo.VoidOrder(voided.Id);

            var summary = repo.GetSummaryForPeriod(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));

            Assert.AreEqual(1,      summary.TotalOrders);
            Assert.AreEqual(11.50m, summary.TotalRevenue);
        }

        // ── GetSummaryForPeriod ───────────────────────────────────────────────

        [TestMethod]
        public void GetSummaryForPeriod_NoOrders_ReturnsZeroes()
        {
            var summary = MakeRepo().GetSummaryForPeriod(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));

            Assert.AreEqual(0,  summary.TotalOrders);
            Assert.AreEqual(0m, summary.TotalRevenue);
        }

        [TestMethod]
        public void GetSummaryForPeriod_OnlyCountsOrdersInRange()
        {
            var repo = MakeRepo();
            repo.Save(MakeRecordOnDate("Jan", new DateTime(2026, 1, 15)));
            repo.Save(MakeRecordOnDate("Feb", new DateTime(2026, 2, 15)));
            repo.Save(MakeRecordOnDate("Mar", new DateTime(2026, 3, 15)));

            var summary = repo.GetSummaryForPeriod(new DateTime(2026, 2, 1), new DateTime(2026, 2, 28));

            Assert.AreEqual(1,      summary.TotalOrders);
            Assert.AreEqual(11.50m, summary.TotalRevenue);
        }

        [TestMethod]
        public void GetSummaryForPeriod_NullBounds_AggregatesAllActiveOrders()
        {
            var repo = MakeRepo();
            repo.Save(MakeRecord("A"));
            repo.Save(MakeRecord("B"));

            var summary = repo.GetSummaryForPeriod(null, null);

            Assert.AreEqual(2,      summary.TotalOrders);
            Assert.AreEqual(23.00m, summary.TotalRevenue);
        }

        // ── GetDailySummaries ─────────────────────────────────────────────────

        [TestMethod]
        public void GetDailySummaries_GroupsOrdersByCalendarDay()
        {
            var repo = MakeRepo();
            repo.Save(MakeRecordOnDate("A1", new DateTime(2026, 1, 5, 10, 0, 0)));
            repo.Save(MakeRecordOnDate("A2", new DateTime(2026, 1, 5, 14, 0, 0)));
            repo.Save(MakeRecordOnDate("B1", new DateTime(2026, 1, 6, 11, 0, 0)));

            List<DailySummary> daily = repo.GetDailySummaries(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));

            Assert.AreEqual(2, daily.Count);
            Assert.AreEqual(2,      daily[0].OrderCount);
            Assert.AreEqual(23.00m, daily[0].Revenue);
            Assert.AreEqual(1,      daily[1].OrderCount);
        }

        [TestMethod]
        public void GetDailySummaries_Empty_ReturnsEmptyList()
        {
            var result = MakeRepo().GetDailySummaries(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetDailySummaries_VoidedOrdersExcluded()
        {
            var repo = MakeRepo();
            var active = MakeRecordOnDate("Active", new DateTime(2026, 1, 5));
            var voided = MakeRecordOnDate("Voided", new DateTime(2026, 1, 5));
            repo.Save(active);
            repo.Save(voided);
            repo.VoidOrder(voided.Id);

            List<DailySummary> daily = repo.GetDailySummaries(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));

            Assert.AreEqual(1, daily.Count);
            Assert.AreEqual(1, daily[0].OrderCount);
        }

        // ── GetTopItems ───────────────────────────────────────────────────────

        [TestMethod]
        public void GetTopItems_ReturnsItemsOrderedByRevenue()
        {
            var repo   = MakeRepo();
            var record = MakeRecord("Bob");
            record.Lines.Add(new OrderLineRecord { Item = "Large Pizza",  Quantity = 1, Price = 10.00m });
            record.Lines.Add(new OrderLineRecord { Item = "Small Pizza",  Quantity = 2, Price = 4.00m  });
            record.Lines.Add(new OrderLineRecord { Item = "Coke - Can",   Quantity = 1, Price = 1.45m  });
            repo.Save(record);

            List<TopItem> items = repo.GetTopItems(null, null, 10);

            // Large Pizza ($10) should come before Small Pizza ($8)
            Assert.IsTrue(items.Count >= 2);
            Assert.AreEqual("Large Pizza", items[0].Item);
        }

        [TestMethod]
        public void GetTopItems_RespectsLimit()
        {
            var repo   = MakeRepo();
            var record = MakeRecord("Anna");
            record.Lines.Add(new OrderLineRecord { Item = "Item A", Quantity = 1, Price = 5.00m });
            record.Lines.Add(new OrderLineRecord { Item = "Item B", Quantity = 1, Price = 4.00m });
            record.Lines.Add(new OrderLineRecord { Item = "Item C", Quantity = 1, Price = 3.00m });
            repo.Save(record);

            List<TopItem> items = repo.GetTopItems(null, null, 2);

            Assert.AreEqual(2, items.Count);
        }

        [TestMethod]
        public void GetTopItems_Empty_ReturnsEmptyList()
        {
            var result = MakeRepo().GetTopItems(null, null, 10);
            Assert.AreEqual(0, result.Count);
        }

        // ── GetPaymentBreakdown ───────────────────────────────────────────────

        [TestMethod]
        public void GetPaymentBreakdown_GroupsByPaymentMethod()
        {
            var repo = MakeRepo();
            var r1 = MakeRecord("A"); r1.PaymentMethod = "Cash";
            var r2 = MakeRecord("B"); r2.PaymentMethod = "Cash";
            var r3 = MakeRecord("C"); r3.PaymentMethod = "Credit Card";
            repo.Save(r1); repo.Save(r2); repo.Save(r3);

            List<PaymentSplit> splits = repo.GetPaymentBreakdown(null, null);

            Assert.AreEqual(2, splits.Count);
            var cash = splits.Find(s => s.PaymentMethod == "Cash");
            var card = splits.Find(s => s.PaymentMethod == "Credit Card");
            Assert.IsNotNull(cash);
            Assert.AreEqual(2, cash.OrderCount);
            Assert.AreEqual(23.00m, cash.Revenue);
            Assert.IsNotNull(card);
            Assert.AreEqual(1, card.OrderCount);
        }

        [TestMethod]
        public void GetPaymentBreakdown_VoidedOrdersExcluded()
        {
            var repo = MakeRepo();
            var active = MakeRecord("Active"); active.PaymentMethod = "Cash";
            var voided = MakeRecord("Voided"); voided.PaymentMethod = "Cash";
            repo.Save(active);
            repo.Save(voided);
            repo.VoidOrder(voided.Id);

            List<PaymentSplit> splits = repo.GetPaymentBreakdown(null, null);

            Assert.AreEqual(1, splits.Count);
            Assert.AreEqual(1, splits[0].OrderCount);
        }

        [TestMethod]
        public void GetPaymentBreakdown_Empty_ReturnsEmptyList()
        {
            var result = MakeRepo().GetPaymentBreakdown(null, null);
            Assert.AreEqual(0, result.Count);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static OrderRecord MakeRecord(string name)
            => MakeRecordOnDate(name, DateTime.Now);

        private static OrderRecord MakeRecordOnDate(string name, DateTime date) => new OrderRecord
        {
            Id            = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
            OrderDate     = date,
            CustomerName  = name,
            Address       = "1 Test St",
            City          = "Wellington",
            Region        = "Wellington",
            PostalCode    = "6011",
            PaymentMethod = "Cash",
            Subtotal      = 10.00m,
            Tax           = 1.50m,
            Total         = 11.50m,
        };
    }
}
