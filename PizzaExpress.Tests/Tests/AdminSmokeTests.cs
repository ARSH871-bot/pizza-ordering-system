using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3;
using WindowsFormsApplication3.Infrastructure;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    [DoNotParallelize]
    [TestClass]
    public class AdminSmokeTests
    {
        // ── Void via UI ───────────────────────────────────────────────────────

        [TestMethod]
        public void OrderHistoryForm_VoidSelectedOrder_PersistsVoidedStatus()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    var repo = new OrderRepository(tempDir);
                    var order = MakeActiveOrder("Alice Baker");
                    repo.Save(order);

                    using (var form = new OrderHistoryForm(repo, settings: null))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        var listView = WinFormsTestHelper.EnumerateControls<ListView>(form)
                            .Single(lv => lv.Columns.Count > 1);
                        Assert.IsTrue(listView.Items.Count > 0, "History list should contain the saved order.");
                        listView.Items[0].Selected = true;

                        using (new WinFormsTestHelper.DialogAutoCloser("Void Order"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "Void Order").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                    }

                    var all = repo.LoadAll();
                    Assert.AreEqual(1, all.Count);
                    Assert.AreEqual("Voided", all[0].Status, "Voided order should persist with Voided status.");
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── Delete via UI ─────────────────────────────────────────────────────

        [TestMethod]
        public void OrderHistoryForm_DeleteSelectedOrder_RemovesFromDatabase()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    var repo = new OrderRepository(tempDir);
                    repo.Save(MakeActiveOrder("Bob Carter"));

                    using (var form = new OrderHistoryForm(repo, settings: null))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        var listView = WinFormsTestHelper.EnumerateControls<ListView>(form)
                            .Single(lv => lv.Columns.Count > 1);
                        Assert.IsTrue(listView.Items.Count > 0, "History list should contain the saved order.");
                        listView.Items[0].Selected = true;

                        using (new WinFormsTestHelper.DialogAutoCloser("Delete Order"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "Delete Order").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                    }

                    var remaining = repo.LoadAll();
                    Assert.AreEqual(0, remaining.Count, "Deleted order should no longer exist in the database.");
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── Direct repo: void ─────────────────────────────────────────────────

        [TestMethod]
        public void OrderRepository_VoidOrder_SetsStatusToVoided()
        {
            string tempDir = CreateTempDir();
            try
            {
                var repo = new OrderRepository(tempDir);
                var order = MakeActiveOrder("Diana Evans");
                repo.Save(order);

                repo.VoidOrder(order.Id);

                var all = repo.LoadAll();
                Assert.AreEqual(1, all.Count);
                Assert.AreEqual("Voided", all[0].Status);
            }
            finally { DeleteTempDir(tempDir); }
        }

        // ── Direct repo: delete ───────────────────────────────────────────────

        [TestMethod]
        public void OrderRepository_DeleteOrder_RemovesRecord()
        {
            string tempDir = CreateTempDir();
            try
            {
                var repo = new OrderRepository(tempDir);
                var order = MakeActiveOrder("Frank Green");
                repo.Save(order);
                Assert.AreEqual(1, repo.LoadAll().Count);

                repo.Delete(order.Id);

                Assert.AreEqual(0, repo.LoadAll().Count);
            }
            finally { DeleteTempDir(tempDir); }
        }

        // ── Backup/restore round-trip ─────────────────────────────────────────

        [TestMethod]
        public void BackupRestoreRoundTrip_WithRealSqliteData_PreservesActiveOrders()
        {
            string tempDir    = CreateTempDir();
            string backupPath = Path.Combine(tempDir, "snapshot.db");
            try
            {
                var repo = new OrderRepository(tempDir);
                var order = MakeActiveOrder("Grace Hall");
                repo.Save(order);

                // Backup while the order is active
                DatabaseBackupService.BackupTo(tempDir, backupPath);

                // Destructive change: void the order in the live DB
                repo.VoidOrder(order.Id);
                Assert.AreEqual("Voided", repo.LoadAll()[0].Status);

                // Restore from the pre-void snapshot
                DatabaseBackupService.RestoreFrom(tempDir, backupPath);

                // Re-open the repository (refreshes the migrated schema view)
                var repoAfterRestore = new OrderRepository(tempDir);
                var restored = repoAfterRestore.LoadAll();

                Assert.AreEqual(1, restored.Count);
                Assert.AreEqual("Active", restored[0].Status, "Restore should bring the order back to Active.");
                Assert.AreEqual("Grace Hall", restored[0].CustomerName);
            }
            finally { DeleteTempDir(tempDir); }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static OrderRecord MakeActiveOrder(string customerName)
        {
            var record = new OrderRecord
            {
                Id            = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant(),
                OrderDate     = DateTime.Now,
                CustomerName  = customerName,
                Address       = "1 Test Street",
                City          = "Auckland",
                Region        = "Auckland",
                PostalCode    = "1010",
                PaymentMethod = "Cash",
                Subtotal      = 10.00m,
                Tax           = 1.50m,
                Total         = 11.50m,
                Status        = "Active",
            };
            record.Lines.Add(new OrderLineRecord { Item = "Small Pizza", Quantity = 1, Price = 10.00m });
            return record;
        }

        private static string CreateTempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(),
                "PizzaExpressAdmin_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static void DeleteTempDir(string dir)
        {
            if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }
}
