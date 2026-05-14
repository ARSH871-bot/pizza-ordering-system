using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3;
using WindowsFormsApplication3.Forms;
using WindowsFormsApplication3.Infrastructure;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    [DoNotParallelize]
    [TestClass]
    public class FormSmokeTests
    {
        [TestMethod]
        public void MajorForms_Construct_AndRenderWithoutThrowing()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart = new CartService(settings);

                    using (var form = new Form1(repo, cart, settings))
                    using (var history = new OrderHistoryForm(repo))
                    using (var settingsForm = new SettingsForm(settings, tempDir))
                    using (var sales = new SalesReportForm(repo))
                    using (var endOfDay = new EndOfDayForm(repo, new DateTime(2026, 5, 11)))
                    using (var pin = new PinLoginForm(settings))
                    {
                        form.Show();
                        history.Show();
                        settingsForm.Show();
                        sales.Show();
                        endOfDay.Show();
                        pin.Show();

                        WinFormsTestHelper.PumpEvents();

                        Assert.IsTrue(form.Visible);
                        Assert.IsTrue(history.Visible);
                        Assert.IsTrue(settingsForm.Visible);
                        Assert.IsTrue(sales.Visible);
                        Assert.IsTrue(endOfDay.Visible);
                        Assert.IsTrue(pin.Visible);
                    }
                }
                finally
                {
                    DeleteTempDataDirectory(tempDir);
                }
            });
        }

        [TestMethod]
        public void Form1_CashCheckoutWorkflow_PersistsMultiPizzaOrder_AndShowsItInHistory()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart = new CartService(settings);

                    using (var form = new Form1(repo, cart, settings))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbSizeMedium").Checked = true;
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbCrustNormal").Checked = true;
                        WinFormsTestHelper.FindByName<NumericUpDown>(form, "nudPizzaQty").Value = 2;
                        WinFormsTestHelper.FindByName<CheckBox>(form, "cbPepperoni").Checked = true;

                        using (new WinFormsTestHelper.DialogAutoCloser("Pizza Added"))
                            WinFormsTestHelper.FindByName<Button>(form, "btnAddPizzaToCart").PerformClick();

                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbSizeLarge").Checked = true;
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbCrustCheesy").Checked = true;
                        WinFormsTestHelper.FindByName<NumericUpDown>(form, "nudPizzaQty").Value = 1;
                        WinFormsTestHelper.FindByName<CheckBox>(form, "cbMushroom").Checked = true;
                        WinFormsTestHelper.FindByName<CheckBox>(form, "cbCoke").Checked = true;
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtQtyCoke").Text = "2";

                        WinFormsTestHelper.FindByName<Button>(form, "btnConfirmOrder").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        var orderList = WinFormsTestHelper.FindByName<ListView>(form, "lvOrder");
                        Assert.IsTrue(orderList.Items.Count >= 5, "The order review should contain both pizzas, toppings, and the drink.");
                        Assert.IsFalse(string.IsNullOrWhiteSpace(WinFormsTestHelper.FindByName<TextBox>(form, "txtTotalDue").Text));

                        WinFormsTestHelper.FindByName<Button>(form, "btnCheckOut").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<TextBox>(form, "txtFirstName").Text = "Jamie";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtLastName").Text = "Taylor";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtAddress").Text = "1 Queen Street";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtCity").Text = "Auckland";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtPostalCode").Text = "1010";
                        WinFormsTestHelper.FindByName<ComboBox>(form, "cboRegion").SelectedItem = "Auckland";
                        WinFormsTestHelper.FindByName<ComboBox>(form, "cboPaymentMethod").SelectedItem = "Cash";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtAmountPaid").Text = "50.00";

                        WinFormsTestHelper.FindByName<Button>(form, "btnPay").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        Assert.IsTrue(WinFormsTestHelper.FindByName<Button>(form, "btnSubmitOrder").Enabled);
                        Assert.IsFalse(string.IsNullOrWhiteSpace(WinFormsTestHelper.FindByName<TextBox>(form, "txtChange").Text));

                        using (new WinFormsTestHelper.DialogAutoCloser("Order Confirmed", "Order Complete"))
                            WinFormsTestHelper.FindByName<Button>(form, "btnSubmitOrder").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                    }

                    var saved = repo.LoadAll();
                    Assert.AreEqual(1, saved.Count);

                    OrderRecord record = saved[0];
                    Assert.AreEqual("Cash", record.PaymentMethod);
                    Assert.AreEqual("Active", record.Status);
                    Assert.IsTrue(record.Total > 0m);
                    Assert.IsTrue(record.Lines.Any(line => line.Item.IndexOf("Medium", StringComparison.OrdinalIgnoreCase) >= 0));
                    Assert.IsTrue(record.Lines.Any(line => line.Item.IndexOf("Large", StringComparison.OrdinalIgnoreCase) >= 0));
                    Assert.IsTrue(record.Lines.Any(line => line.Item.IndexOf("Coke", StringComparison.OrdinalIgnoreCase) >= 0));

                    using (var history = new OrderHistoryForm(repo))
                    {
                        history.Show();
                        WinFormsTestHelper.PumpEvents();

                        var listView = WinFormsTestHelper.EnumerateControls<ListView>(history).Single();
                        Assert.IsTrue(
                            listView.Items.Cast<ListViewItem>().Any(item => item.Tag is OrderRecord),
                            "Order history should show the newly submitted order.");
                    }
                }
                finally
                {
                    DeleteTempDataDirectory(tempDir);
                }
            });
        }

        [TestMethod]
        public void Form1_PromoCheckoutWorkflow_AppliesDiscount_AndPersistsDiscountMetadata()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart = new CartService(settings);

                    decimal totalBeforeDiscount;
                    decimal totalAfterDiscount;

                    using (var form = new Form1(repo, cart, settings))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbSizeLarge").Checked = true;
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbCrustCheesy").Checked = true;
                        WinFormsTestHelper.FindByName<CheckBox>(form, "cbExtraCheese").Checked = true;
                        WinFormsTestHelper.FindByName<Button>(form, "btnConfirmOrder").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<Button>(form, "btnCheckOut").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<TextBox>(form, "txtFirstName").Text = "Morgan";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtLastName").Text = "Lee";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtAddress").Text = "2 Customs Street";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtCity").Text = "Auckland";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtPostalCode").Text = "1010";
                        WinFormsTestHelper.FindByName<ComboBox>(form, "cboRegion").SelectedItem = "Auckland";
                        WinFormsTestHelper.FindByName<ComboBox>(form, "cboPaymentMethod").SelectedItem = "Promo Card";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtCardOrPromo").Text = "PIZZA10";

                        totalBeforeDiscount = ParseCurrency(WinFormsTestHelper.FindByName<TextBox>(form, "txtTotalDue").Text);

                        using (new WinFormsTestHelper.DialogAutoCloser("Promo Applied"))
                            WinFormsTestHelper.FindByName<Button>(form, "btnPay").PerformClick();

                        WinFormsTestHelper.PumpEvents();

                        totalAfterDiscount = ParseCurrency(WinFormsTestHelper.FindByName<TextBox>(form, "txtAmountDue").Text);
                        Assert.IsTrue(totalAfterDiscount < totalBeforeDiscount);
                        Assert.IsTrue(WinFormsTestHelper.FindByName<Button>(form, "btnSubmitOrder").Enabled);

                        using (new WinFormsTestHelper.DialogAutoCloser("Order Confirmed", "Order Complete"))
                            WinFormsTestHelper.FindByName<Button>(form, "btnSubmitOrder").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                    }

                    var saved = repo.LoadAll();
                    Assert.AreEqual(1, saved.Count);
                    Assert.AreEqual("Promo Card", saved[0].PaymentMethod);
                    Assert.AreEqual("PIZZA10", saved[0].DiscountDescription);
                    Assert.IsTrue(saved[0].Discount > 0m);
                    Assert.AreEqual(totalAfterDiscount, saved[0].Total);
                }
                finally
                {
                    DeleteTempDataDirectory(tempDir);
                }
            });
        }

        [TestMethod]
        public void SettingsForm_SaveChanges_UpdatesRuntimePricing_AndRendersBackupControls()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var settings = new SettingsRepository(tempDir);

                    using (var form = new SettingsForm(settings, tempDir))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        Assert.IsNotNull(WinFormsTestHelper.FindByTextPrefix<Button>(form, "Backup DB"));
                        Assert.IsNotNull(WinFormsTestHelper.FindByTextPrefix<Button>(form, "Restore DB"));
                        Assert.IsNotNull(WinFormsTestHelper.FindByTextPrefix<Button>(form, "View Auto-Backups"));

                        // The DB-size label shows "DB: N KB" or "DB: —" depending on file size.
                        Label dbSizeLabel = WinFormsTestHelper
                            .EnumerateControls<Label>(form)
                            .Single(label => (label.Text ?? string.Empty).StartsWith("DB:", StringComparison.OrdinalIgnoreCase));
                        Assert.IsNotNull(dbSizeLabel);

                        DataGridView grid = WinFormsTestHelper.EnumerateControls<DataGridView>(form).Single();
                        DataGridViewRow drinkPriceRow = grid.Rows
                            .Cast<DataGridViewRow>()
                            .Single(row => string.Equals(row.Tag as string, "DrinkCanPrice", StringComparison.Ordinal));

                        drinkPriceRow.Cells["Value"].Value = "4.75";

                        using (new WinFormsTestHelper.DialogAutoCloser("Saved"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "Save Changes").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                    }

                    Assert.AreEqual("4.75", settings.Get("DrinkCanPrice"));
                    Assert.AreEqual(4.75m, new CartService(settings).GetDrinkCanPrice());
                }
                finally
                {
                    DeleteTempDataDirectory(tempDir);
                }
            });
        }

        private static string CreateTempDataDirectory()
        {
            string dir = Path.Combine(Path.GetTempPath(), "PizzaExpressForms_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static void DeleteTempDataDirectory(string tempDir)
        {
            if (!string.IsNullOrWhiteSpace(tempDir) && Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        private static decimal ParseCurrency(string text)
        {
            decimal value;
            Assert.IsTrue(
                decimal.TryParse(text, NumberStyles.Currency, new CultureInfo("en-NZ"), out value),
                $"Expected a currency value but received '{text}'.");
            return value;
        }
    }
}
