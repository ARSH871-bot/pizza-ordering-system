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
    public class EndOfDayFormTests
    {
        // ── With real order data ──────────────────────────────────────────────

        [TestMethod]
        public void EndOfDayForm_WithOrders_PopulatesKpisAndPaymentList()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);

                    // Save two orders for today so payment and item lists are non-empty
                    repo.Save(MakeOrder("Cash",   15.00m));
                    repo.Save(MakeOrder("Credit Card", 22.50m));

                    using (var form = new EndOfDayForm(repo, DateTime.Today))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        // KPI labels should reflect the two orders
                        var lblOrders  = WinFormsTestHelper.GetPrivateField<Label>(form, "_lblOrders");
                        var lblRevenue = WinFormsTestHelper.GetPrivateField<Label>(form, "_lblRevenue");
                        Assert.AreEqual("2", lblOrders.Text, "Order count should be 2.");
                        Assert.IsFalse(string.IsNullOrWhiteSpace(lblRevenue.Text),
                            "Revenue label should be populated.");

                        // Payment breakdown list should have 2 rows (one per payment method)
                        var lvPayments = WinFormsTestHelper.GetPrivateField<ListView>(form, "_lvPayments");
                        Assert.AreEqual(2, lvPayments.Items.Count,
                            "Payment list should show one row per payment method.");

                        // Top items list should have at least one row
                        var lvItems = WinFormsTestHelper.GetPrivateField<ListView>(form, "_lvItems");
                        Assert.IsTrue(lvItems.Items.Count >= 1,
                            "Top items list should have at least one entry.");
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── Print Report ──────────────────────────────────────────────────────

        [TestMethod]
        public void EndOfDayForm_PrintReport_OpensPreviewAndCanBeClosed()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);

                    using (var form = new EndOfDayForm(repo, DateTime.Today))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        // DialogAutoCloser sends WM_CLOSE to dismiss the PrintPreviewDialog
                        using (new WinFormsTestHelper.DialogAutoCloser("Print Preview"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "Print Report").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                        Assert.IsTrue(form.Visible, "Form should remain open after print preview closed.");
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
            record.Lines.Add(new OrderLineRecord
            {
                Item     = "Large Pizza",
                Quantity = 1,
                Price    = total,
            });
            return record;
        }

        private static string CreateTempDir()
        {
            string dir = Path.Combine(
                Path.GetTempPath(),
                "PizzaExpressEOD_" + Guid.NewGuid().ToString("N"));
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
