using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Forms;
using WindowsFormsApplication3.Infrastructure;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    [DoNotParallelize]
    [TestClass]
    public class SalesReportFormTests
    {
        // ── Run Report button ─────────────────────────────────────────────────

        [TestMethod]
        public void SalesReportForm_RunReport_WithOrders_PopulatesKpis()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);
                    repo.Save(MakeOrder("Cash",        18.00m));
                    repo.Save(MakeOrder("Credit Card", 25.00m));

                    using (var form = new SalesReportForm(repo))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByTextPrefix<Button>(form, "Run Report").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        var lblOrders = WinFormsTestHelper.GetPrivateField<Label>(form, "_lblOrders");
                        Assert.AreEqual("2", lblOrders.Text, "Order count should be 2 after Run Report.");
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── Quick-date buttons ────────────────────────────────────────────────

        [TestMethod]
        public void SalesReportForm_TodayButton_SetsDateRangeAndRunsReport()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);

                    using (var form = new SalesReportForm(repo))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByTextPrefix<Button>(form, "Today").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        var dtpFrom = WinFormsTestHelper.GetPrivateField<DateTimePicker>(form, "_dtpFrom");
                        var dtpTo   = WinFormsTestHelper.GetPrivateField<DateTimePicker>(form, "_dtpTo");
                        Assert.AreEqual(DateTime.Today, dtpFrom.Value.Date);
                        Assert.AreEqual(DateTime.Today, dtpTo.Value.Date);
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        [TestMethod]
        public void SalesReportForm_ThisWeekButton_SetsFromToStartOfWeek()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);

                    using (var form = new SalesReportForm(repo))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByTextPrefix<Button>(form, "This Week").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        var dtpFrom = WinFormsTestHelper.GetPrivateField<DateTimePicker>(form, "_dtpFrom");
                        var dtpTo   = WinFormsTestHelper.GetPrivateField<DateTimePicker>(form, "_dtpTo");
                        Assert.IsTrue(dtpFrom.Value.Date <= DateTime.Today);
                        Assert.AreEqual(DateTime.Today, dtpTo.Value.Date);
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        [TestMethod]
        public void SalesReportForm_ThisMonthButton_SetsFromToFirstOfMonth()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);

                    using (var form = new SalesReportForm(repo))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByTextPrefix<Button>(form, "This Month").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        var dtpFrom = WinFormsTestHelper.GetPrivateField<DateTimePicker>(form, "_dtpFrom");
                        var expected = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        Assert.AreEqual(expected, dtpFrom.Value.Date);
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static OrderRecord MakeOrder(string paymentMethod, decimal total)
        {
            var record = new OrderRecord
            {
                Id            = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant(),
                OrderDate     = DateTime.Now,
                CustomerName  = "Test Customer",
                Address       = "1 Test Street",
                City          = "Auckland",
                Region        = "Auckland",
                PostalCode    = "1010",
                PaymentMethod = paymentMethod,
                Subtotal      = Math.Round(total / 1.15m, 2),
                Tax           = Math.Round(total - total / 1.15m, 2),
                Total         = total,
                Status        = "Active",
            };
            record.Lines.Add(new OrderLineRecord { Item = "Large Pizza", Quantity = 1, Price = total });
            return record;
        }

        private static string CreateTempDir()
        {
            string dir = Path.Combine(
                Path.GetTempPath(),
                "PizzaExpressSalesRpt_" + Guid.NewGuid().ToString("N"));
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
