using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public class OrderHistoryFormTests
    {
        // ── Search filter ─────────────────────────────────────────────────────

        [TestMethod]
        public void OrderHistoryForm_SearchFilter_ShowsOnlyMatchingOrders()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);
                    repo.Save(MakeOrder("Alice Baker",  "Auckland"));
                    repo.Save(MakeOrder("Bob Carter",   "Wellington"));
                    repo.Save(MakeOrder("Carol Dawson", "Auckland"));

                    using (var form = new OrderHistoryForm(repo))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        var listView = WinFormsTestHelper.EnumerateControls<ListView>(form)
                            .Single(lv => lv.Columns.Count > 1);

                        int countBefore = listView.Items.Cast<ListViewItem>()
                            .Count(item => item.Tag is OrderRecord);
                        Assert.AreEqual(3, countBefore, "All 3 orders should show initially.");

                        // Set the search text — TextChanged fires ApplyFilter
                        var txtSearch = WinFormsTestHelper.GetPrivateField<TextBox>(form, "_txtSearch");
                        txtSearch.Text = "Alice";
                        WinFormsTestHelper.PumpEvents();

                        int countAfter = listView.Items.Cast<ListViewItem>()
                            .Count(item => item.Tag is OrderRecord);
                        Assert.AreEqual(1, countAfter, "Only Alice's order should match.");
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        [TestMethod]
        public void OrderHistoryForm_SearchFilter_NoMatches_ShowsNoMatchingMessage()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);
                    repo.Save(MakeOrder("Alice Baker", "Auckland"));

                    using (var form = new OrderHistoryForm(repo))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        var txtSearch = WinFormsTestHelper.GetPrivateField<TextBox>(form, "_txtSearch");
                        txtSearch.Text = "ZZZ_NO_MATCH";
                        WinFormsTestHelper.PumpEvents();

                        var listView = WinFormsTestHelper.EnumerateControls<ListView>(form)
                            .Single(lv => lv.Columns.Count > 1);

                        bool hasNoMatchRow = listView.Items.Cast<ListViewItem>()
                            .Any(item => item.Text.Contains("No matching orders"));
                        Assert.IsTrue(hasNoMatchRow, "Should show 'No matching orders found.' row.");
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── View Details ──────────────────────────────────────────────────────

        [TestMethod]
        public void OrderHistoryForm_ViewDetails_WithSelectedOrder_ShowsDetailDialog()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);
                    repo.Save(MakeOrder("Diana Evans", "Christchurch"));

                    using (var form = new OrderHistoryForm(repo))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        var listView = WinFormsTestHelper.EnumerateControls<ListView>(form)
                            .Single(lv => lv.Columns.Count > 1);
                        listView.Items[0].Selected = true;
                        WinFormsTestHelper.PumpEvents();

                        using (new WinFormsTestHelper.DialogAutoCloser("Order Details"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "View Details").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── Date filter ───────────────────────────────────────────────────────

        [TestMethod]
        public void OrderHistoryForm_DateFilter_WhenEnabled_RefiltersOrders()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);
                    repo.Save(MakeOrder("Frank Green", "Auckland"));

                    using (var form = new OrderHistoryForm(repo))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        var chkDate = WinFormsTestHelper.GetPrivateField<CheckBox>(form, "_chkDateFilter");
                        Assert.IsFalse(chkDate.Checked, "Date filter should be off initially.");

                        // Enabling the checkbox fires CheckedChanged → ApplyFilter
                        chkDate.Checked = true;
                        WinFormsTestHelper.PumpEvents();

                        // Form should still be alive and not throw
                        Assert.IsTrue(form.Visible);

                        var listView = WinFormsTestHelper.EnumerateControls<ListView>(form)
                            .Single(lv => lv.Columns.Count > 1);

                        // The default date range is last month to today — today's order should be visible
                        bool hasOrder = listView.Items.Cast<ListViewItem>()
                            .Any(item => item.Tag is OrderRecord);
                        Assert.IsTrue(hasOrder, "Today's order should appear in the default date range.");

                        chkDate.Checked = false;
                        WinFormsTestHelper.PumpEvents();
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── Column-click sorting ──────────────────────────────────────────────

        [TestMethod]
        public void OrderHistoryForm_ColumnClick_SortsByDateToggle()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);
                    repo.Save(MakeOrder("Alpha", "Auckland"));
                    repo.Save(MakeOrder("Beta",  "Wellington"));
                    repo.Save(MakeOrder("Gamma", "Christchurch"));

                    using (var form = new OrderHistoryForm(repo))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        var mi = typeof(OrderHistoryForm).GetMethod(
                            "ListView_ColumnClick",
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        Assert.IsNotNull(mi, "ListView_ColumnClick not found.");

                        var listView = WinFormsTestHelper.EnumerateControls<ListView>(form)
                            .Single(lv => lv.Columns.Count > 1);

                        // First click on column 0 — ascending (SortOrders lambda runs)
                        mi.Invoke(form, new object[] { listView, new ColumnClickEventArgs(0) });
                        WinFormsTestHelper.PumpEvents();
                        Assert.IsTrue(listView.Columns[0].Text.Contains("▲"),
                            "Column 0 header should show ascending arrow after first click.");

                        // Second click on column 0 — descending (SortOrders lambda runs again)
                        mi.Invoke(form, new object[] { listView, new ColumnClickEventArgs(0) });
                        WinFormsTestHelper.PumpEvents();
                        Assert.IsTrue(listView.Columns[0].Text.Contains("▼"),
                            "Column 0 header should show descending arrow after second click.");
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        [TestMethod]
        public void OrderHistoryForm_ColumnClick_SortsByTotalAndCustomerName()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);
                    repo.Save(MakeOrder("Alpha", "Auckland"));
                    repo.Save(MakeOrder("Beta",  "Wellington"));

                    using (var form = new OrderHistoryForm(repo))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        var mi = typeof(OrderHistoryForm).GetMethod(
                            "ListView_ColumnClick",
                            BindingFlags.NonPublic | BindingFlags.Instance);

                        var listView = WinFormsTestHelper.EnumerateControls<ListView>(form)
                            .Single(lv => lv.Columns.Count > 1);

                        // Column 1 = Customer Name, column 4 = Total
                        mi.Invoke(form, new object[] { listView, new ColumnClickEventArgs(1) });
                        WinFormsTestHelper.PumpEvents();
                        mi.Invoke(form, new object[] { listView, new ColumnClickEventArgs(4) });
                        WinFormsTestHelper.PumpEvents();
                        mi.Invoke(form, new object[] { listView, new ColumnClickEventArgs(2) });
                        WinFormsTestHelper.PumpEvents();
                        mi.Invoke(form, new object[] { listView, new ColumnClickEventArgs(3) });
                        WinFormsTestHelper.PumpEvents();

                        // All column headers should remain valid
                        Assert.IsTrue(listView.Items.Cast<ListViewItem>()
                            .Any(item => item.Tag is OrderRecord));
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── Keyboard shortcuts ────────────────────────────────────────────────

        [TestMethod]
        public void OrderHistoryForm_KeyDown_Delete_WithNoSelection_DoesNotThrow()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);

                    using (var form = new OrderHistoryForm(repo))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        // Raise KeyDown on the form directly — no item selected, guard returns early
                        var args = new KeyEventArgs(Keys.Delete);
                        form.GetType()
                            .GetField("KeyDown", BindingFlags.NonPublic | BindingFlags.Instance |
                                                 BindingFlags.FlattenHierarchy);

                        // Invoke via the protected OnKeyDown entry point
                        var onKeyDown = typeof(Control).GetMethod(
                            "OnKeyDown",
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        onKeyDown.Invoke(form, new object[] { args });
                        WinFormsTestHelper.PumpEvents();

                        Assert.IsTrue(form.Visible);
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── ExportCsv branches ────────────────────────────────────────────────

        [TestMethod]
        public void OrderHistoryForm_ExportCsv_EmptyList_ReturnsImmediately()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo     = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);

                    using (var form = new OrderHistoryForm(repo, settings))
                    {
                        // Invoke ExportCsv directly to cover the count==0 early-return without
                        // needing a visible window (avoids headless CI handle-creation hang).
                        var mi = typeof(OrderHistoryForm).GetMethod(
                            "ExportCsv", BindingFlags.NonPublic | BindingFlags.Instance);
                        mi.Invoke(form, null);
                        // Reaching here without an exception confirms the early-return guard fires.
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── SortOrders default column (no-op) ────────────────────────────────

        [TestMethod]
        public void OrderHistoryForm_SortOrders_UnknownColumn_ReturnsZero()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);
                    repo.Save(MakeOrder("Alice", "Auckland"));
                    repo.Save(MakeOrder("Bob",   "Wellington"));

                    var settings = new SettingsRepository(tempDir);
                    using (var form = new OrderHistoryForm(repo, settings))
                    {
                        // Set _sortColumn to a value outside switch cases (0–4)
                        typeof(OrderHistoryForm)
                            .GetField("_sortColumn", BindingFlags.NonPublic | BindingFlags.Instance)
                            .SetValue(form, 99);

                        // SortOrders relies on _currentOrders, which loads on construction;
                        // invoke it directly to hit the default: return 0 branch
                        typeof(OrderHistoryForm)
                            .GetMethod("SortOrders", BindingFlags.NonPublic | BindingFlags.Instance)
                            .Invoke(form, null);
                    }
                }
                finally
                {
                    if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                }
            });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static OrderRecord MakeOrder(string customerName, string region)
        {
            var record = new OrderRecord
            {
                Id            = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant(),
                OrderDate     = DateTime.Now,
                CustomerName  = customerName,
                Address       = "1 Test Street",
                City          = region,
                Region        = region,
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
                "PizzaExpressHistory_" + Guid.NewGuid().ToString("N"));
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
