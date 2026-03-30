using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Infrastructure;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    [TestClass]
    public class DatabaseMigratorTests
    {
        private string _tempDir;

        [TestInitialize]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "PizzaExpressMigratorTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TestCleanup]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        [TestMethod]
        public void Run_FreshDirectory_CreatesDatabaseAndSeedsSettings()
        {
            DatabaseMigrator.Run(_tempDir);

            var settings = new SettingsRepository(_tempDir);

            Assert.IsTrue(File.Exists(Path.Combine(_tempDir, "orders.db")));
            Assert.AreEqual("30", settings.Get("DeliveryMinutes"));
            Assert.AreEqual(string.Empty, settings.Get("StaffPin"));
        }

        [TestMethod]
        public void Run_CanBeCalledRepeatedly_WithoutBreakingRepositoryUsage()
        {
            DatabaseMigrator.Run(_tempDir);
            DatabaseMigrator.Run(_tempDir);

            var repo = new OrderRepository(_tempDir);
            var record = new OrderRecord
            {
                Id = "MIGRATE1",
                OrderDate = new DateTime(2026, 3, 30, 9, 0, 0),
                CustomerName = "Repeat Safe",
                PaymentMethod = "Cash",
                Subtotal = 10.00m,
                Tax = 1.50m,
                Total = 9.50m,
                Discount = 2.00m,
                DiscountDescription = "PROMO",
            };
            record.Lines.Add(new OrderLineRecord
            {
                Item = "Large Pizza",
                Quantity = 1,
                Price = 10.00m,
            });

            repo.Save(record);
            var loaded = repo.LoadAll();

            Assert.AreEqual(1, loaded.Count);
            Assert.AreEqual(2.00m, loaded[0].Discount);
            Assert.AreEqual("PROMO", loaded[0].DiscountDescription);
        }
    }
}
