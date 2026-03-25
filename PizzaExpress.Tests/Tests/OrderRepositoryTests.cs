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
