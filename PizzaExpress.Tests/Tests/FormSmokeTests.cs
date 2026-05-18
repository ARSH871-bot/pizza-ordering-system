using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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

                    using (var form = new Form1(repo, cart, settings, showReceiptDialogs: false))
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

                    using (var form = new Form1(repo, cart, settings, showReceiptDialogs: false))
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

        [TestMethod]
        public void Form1_CreditCardCheckout_MasksReferenceAndPersistsOrder()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo     = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart     = new CartService(settings);

                    using (var form = new Form1(repo, cart, settings, showReceiptDialogs: false))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbSizeSmall").Checked  = true;
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbCrustNormal").Checked = true;
                        WinFormsTestHelper.FindByName<Button>(form, "btnConfirmOrder").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<Button>(form, "btnCheckOut").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<TextBox>(form, "txtFirstName").Text  = "Taylor";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtLastName").Text   = "Smith";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtAddress").Text    = "5 Lambton Quay";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtCity").Text       = "Wellington";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtPostalCode").Text = "6011";
                        WinFormsTestHelper.FindByName<ComboBox>(form, "cboRegion").SelectedItem = "Wellington";

                        // Select Credit Card: must enable the reference field
                        WinFormsTestHelper.FindByName<ComboBox>(form, "cboPaymentMethod").SelectedItem = "Credit Card";
                        WinFormsTestHelper.PumpEvents();
                        Assert.IsTrue(WinFormsTestHelper.FindByName<TextBox>(form, "txtCardOrPromo").Enabled,
                            "txtCardOrPromo should be enabled for Credit Card");

                        WinFormsTestHelper.FindByName<TextBox>(form, "txtCardOrPromo").Text  = "4111111111111111";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtAmountPaid").Text   = "20.00";

                        WinFormsTestHelper.FindByName<Button>(form, "btnPay").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        Assert.IsTrue(WinFormsTestHelper.FindByName<Button>(form, "btnSubmitOrder").Enabled);

                        WinFormsTestHelper.FindByName<Button>(form, "btnSubmitOrder").PerformClick();
                        WinFormsTestHelper.PumpEvents();
                    }

                    var saved = repo.LoadAll();
                    Assert.AreEqual(1, saved.Count);
                    Assert.AreEqual("Credit Card", saved[0].PaymentMethod);
                    Assert.AreEqual("****1111",    saved[0].PaymentReference, "Card number should be masked");
                }
                finally
                {
                    DeleteTempDataDirectory(tempDir);
                }
            });
        }

        [TestMethod]
        public void Form1_ClearOrder_WhenItemsPresent_RemovesAllItemsOnConfirm()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo     = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart     = new CartService(settings);

                    using (var form = new Form1(repo, cart, settings, showReceiptDialogs: false))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        // Build an order first
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbSizeSmall").Checked   = true;
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbCrustNormal").Checked = true;
                        WinFormsTestHelper.FindByName<Button>(form, "btnConfirmOrder").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        var lvOrder = WinFormsTestHelper.FindByName<ListView>(form, "lvOrder");
                        Assert.IsTrue(lvOrder.Items.Count > 0, "Order should have items after Confirm");

                        // Click Clear Order and confirm Yes (dialog title is "Clear Order")
                        using (new WinFormsTestHelper.DialogAutoCloser("Clear Order"))
                            WinFormsTestHelper.FindByName<Button>(form, "btnClearOrder").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                        Assert.AreEqual(0, lvOrder.Items.Count, "Order should be empty after Clear + Yes");
                    }
                }
                finally
                {
                    DeleteTempDataDirectory(tempDir);
                }
            });
        }

        [TestMethod]
        public void Form1_OrderAgain_NavigatesFromTab2BackToTab1()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo     = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart     = new CartService(settings);

                    using (var form = new Form1(repo, cart, settings, showReceiptDialogs: false))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbSizeSmall").Checked   = true;
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbCrustNormal").Checked = true;
                        WinFormsTestHelper.FindByName<Button>(form, "btnConfirmOrder").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        var tabControl = WinFormsTestHelper.FindByName<TabControl>(form, "tabControl1");
                        Assert.AreEqual(1, tabControl.SelectedIndex, "Should be on Tab 2 after Confirm");

                        WinFormsTestHelper.FindByName<Button>(form, "btnOrderAgain").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        Assert.AreEqual(0, tabControl.SelectedIndex, "Order Again should return to Tab 1");
                    }
                }
                finally
                {
                    DeleteTempDataDirectory(tempDir);
                }
            });
        }

        [TestMethod]
        public void Form1_GoBack_NavigatesFromTab3BackToTab2()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo     = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart     = new CartService(settings);

                    using (var form = new Form1(repo, cart, settings, showReceiptDialogs: false))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbSizeSmall").Checked   = true;
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbCrustNormal").Checked = true;
                        WinFormsTestHelper.FindByName<Button>(form, "btnConfirmOrder").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<Button>(form, "btnCheckOut").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        var tabControl = WinFormsTestHelper.FindByName<TabControl>(form, "tabControl1");
                        Assert.AreEqual(2, tabControl.SelectedIndex, "Should be on Tab 3 after Check Out");

                        WinFormsTestHelper.FindByName<Button>(form, "btnGoBack").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        Assert.AreEqual(1, tabControl.SelectedIndex, "Go Back should return to Tab 2");
                    }
                }
                finally
                {
                    DeleteTempDataDirectory(tempDir);
                }
            });
        }

        [TestMethod]
        public void Form1_DebitCardCheckout_MasksReferenceAndPersistsOrder()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo     = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart     = new CartService(settings);

                    using (var form = new Form1(repo, cart, settings, showReceiptDialogs: false))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbSizeSmall").Checked   = true;
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbCrustNormal").Checked = true;
                        WinFormsTestHelper.FindByName<Button>(form, "btnConfirmOrder").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<Button>(form, "btnCheckOut").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<TextBox>(form, "txtFirstName").Text  = "Alex";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtLastName").Text   = "Jordan";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtAddress").Text    = "10 Willis Street";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtCity").Text       = "Wellington";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtPostalCode").Text = "6011";
                        WinFormsTestHelper.FindByName<ComboBox>(form, "cboRegion").SelectedItem = "Wellington";

                        WinFormsTestHelper.FindByName<ComboBox>(form, "cboPaymentMethod").SelectedItem = "Debit Card";
                        WinFormsTestHelper.PumpEvents();
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtCardOrPromo").Text  = "5500123456789012";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtAmountPaid").Text   = "25.00";

                        WinFormsTestHelper.FindByName<Button>(form, "btnPay").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        Assert.IsTrue(WinFormsTestHelper.FindByName<Button>(form, "btnSubmitOrder").Enabled);

                        WinFormsTestHelper.FindByName<Button>(form, "btnSubmitOrder").PerformClick();
                        WinFormsTestHelper.PumpEvents();
                    }

                    var saved = repo.LoadAll();
                    Assert.AreEqual(1, saved.Count);
                    Assert.AreEqual("Debit Card", saved[0].PaymentMethod);
                    Assert.AreEqual("****9012", saved[0].PaymentReference, "Debit card number should be masked");
                }
                finally
                {
                    DeleteTempDataDirectory(tempDir);
                }
            });
        }

        [TestMethod]
        public void Form1_CashUnderpayment_DisablesSubmitOrder()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo     = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart     = new CartService(settings);

                    using (var form = new Form1(repo, cart, settings, showReceiptDialogs: false))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbSizeLarge").Checked   = true;
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbCrustNormal").Checked = true;
                        WinFormsTestHelper.FindByName<Button>(form, "btnConfirmOrder").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<Button>(form, "btnCheckOut").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<TextBox>(form, "txtFirstName").Text  = "Sam";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtLastName").Text   = "Rivera";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtAddress").Text    = "3 Queen Street";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtCity").Text       = "Auckland";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtPostalCode").Text = "1010";
                        WinFormsTestHelper.FindByName<ComboBox>(form, "cboRegion").SelectedItem = "Auckland";
                        WinFormsTestHelper.FindByName<ComboBox>(form, "cboPaymentMethod").SelectedItem = "Cash";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtAmountPaid").Text = "0.01";

                        using (new WinFormsTestHelper.DialogAutoCloser("Payment Error"))
                            WinFormsTestHelper.FindByName<Button>(form, "btnPay").PerformClick();

                        WinFormsTestHelper.PumpEvents();

                        Assert.IsFalse(WinFormsTestHelper.FindByName<Button>(form, "btnSubmitOrder").Enabled,
                            "Submit Order should remain disabled when payment is insufficient");
                    }
                }
                finally
                {
                    DeleteTempDataDirectory(tempDir);
                }
            });
        }

        [TestMethod]
        public void Form1_SubmitOrder_WithReceiptDialogs_SkipAndOrderAgain_ResetsToTab1()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo     = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart     = new CartService(settings);

                    // Production constructor: showReceiptDialogs = true
                    using (var form = new Form1(repo, cart, settings))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbSizeSmall").Checked   = true;
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbCrustNormal").Checked = true;
                        WinFormsTestHelper.FindByName<Button>(form, "btnConfirmOrder").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<Button>(form, "btnCheckOut").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<TextBox>(form, "txtFirstName").Text  = "Jo";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtLastName").Text   = "Kim";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtAddress").Text    = "1 Test Street";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtCity").Text       = "Auckland";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtPostalCode").Text = "1010";
                        WinFormsTestHelper.FindByName<ComboBox>(form, "cboRegion").SelectedItem = "Auckland";
                        WinFormsTestHelper.FindByName<ComboBox>(form, "cboPaymentMethod").SelectedItem = "Cash";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtAmountPaid").Text = "50.00";

                        WinFormsTestHelper.FindByName<Button>(form, "btnPay").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        Assert.IsTrue(WinFormsTestHelper.FindByName<Button>(form, "btnSubmitOrder").Enabled,
                            "btnSubmitOrder must be enabled before clicking.");

                        // Receipt options dialog title: "Order Confirmed — Receipt Options"
                        // Dismisses via WM_CLOSE (Skip path).
                        // Order Complete dialog title: "Order Complete"
                        // Dismisses via IDYES (Order Again path) — resets form to Tab 1.
                        using (new WinFormsTestHelper.DialogAutoCloser("Order Confirmed", "Order Complete"))
                            WinFormsTestHelper.FindByName<Button>(form, "btnSubmitOrder").PerformClick();

                        WinFormsTestHelper.PumpEvents();

                        var tabControl = WinFormsTestHelper.FindByName<TabControl>(form, "tabControl1");
                        Assert.AreEqual(0, tabControl.SelectedIndex,
                            "Form should return to Tab 1 after Order Again in Order Complete dialog.");
                    }

                    var saved = repo.LoadAll();
                    Assert.AreEqual(1, saved.Count, "Order should have been persisted.");
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void Form1_ReceiptDialog_SkipButton_ClosesDialogAndPromptsOrderComplete()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo     = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart     = new CartService(settings);

                    using (var form = new Form1(repo, cart, settings))  // showReceiptDialogs = true
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbSizeSmall").Checked   = true;
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbCrustNormal").Checked = true;
                        WinFormsTestHelper.FindByName<Button>(form, "btnConfirmOrder").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<Button>(form, "btnCheckOut").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<TextBox>(form, "txtFirstName").Text  = "Lee";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtLastName").Text   = "Park";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtAddress").Text    = "9 High Street";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtCity").Text       = "Auckland";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtPostalCode").Text = "1010";
                        WinFormsTestHelper.FindByName<ComboBox>(form, "cboRegion").SelectedItem = "Auckland";
                        WinFormsTestHelper.FindByName<ComboBox>(form, "cboPaymentMethod").SelectedItem = "Cash";
                        WinFormsTestHelper.FindByName<TextBox>(form, "txtAmountPaid").Text = "50.00";

                        WinFormsTestHelper.FindByName<Button>(form, "btnPay").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        Assert.IsTrue(WinFormsTestHelper.FindByName<Button>(form, "btnSubmitOrder").Enabled);

                        // Click "Skip" inside the receipt dialog (covers btnSkip.Click lambda),
                        // then IDYES on "Order Complete" resets the form to Tab 1.
                        using (new WinFormsTestHelper.DialogButtonClicker("Order Confirmed", "Skip"))
                        using (new WinFormsTestHelper.DialogAutoCloser("Order Complete"))
                            WinFormsTestHelper.FindByName<Button>(form, "btnSubmitOrder").PerformClick();

                        WinFormsTestHelper.PumpEvents();

                        var tabControl = WinFormsTestHelper.FindByName<TabControl>(form, "tabControl1");
                        Assert.AreEqual(0, tabControl.SelectedIndex,
                            "Form should return to Tab 1 after Order Again.");
                    }

                    Assert.AreEqual(1, repo.LoadAll().Count, "Order should be persisted.");
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void Form1_HistoryButton_OpensOrderHistoryDialog()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo     = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart     = new CartService(settings);

                    using (var form = new Form1(repo, cart, settings, showReceiptDialogs: false))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        // Navigate to Tab 2 so the Order History button is accessible
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbSizeSmall").Checked   = true;
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbCrustNormal").Checked = true;
                        WinFormsTestHelper.FindByName<Button>(form, "btnConfirmOrder").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        using (new WinFormsTestHelper.DialogAutoCloser("Order History"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "Order History").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                        Assert.IsTrue(form.Visible, "Form1 should still be visible after closing Order History.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void Form1_SalesReportButton_OpensSalesReportDialog()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo     = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart     = new CartService(settings);

                    using (var form = new Form1(repo, cart, settings, showReceiptDialogs: false))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbSizeSmall").Checked   = true;
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbCrustNormal").Checked = true;
                        WinFormsTestHelper.FindByName<Button>(form, "btnConfirmOrder").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        using (new WinFormsTestHelper.DialogAutoCloser("Sales Report"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "Sales Report").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                        Assert.IsTrue(form.Visible);
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void Form1_EndOfDayButton_OpensEndOfDayDialog()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo     = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart     = new CartService(settings);

                    using (var form = new Form1(repo, cart, settings, showReceiptDialogs: false))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbSizeSmall").Checked   = true;
                        WinFormsTestHelper.FindByName<RadioButton>(form, "rbCrustNormal").Checked = true;
                        WinFormsTestHelper.FindByName<Button>(form, "btnConfirmOrder").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        using (new WinFormsTestHelper.DialogAutoCloser("End of Day"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "End of Day").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                        Assert.IsTrue(form.Visible);
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void Form1_ExitButton_Yes_ClosesForm()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo     = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart     = new CartService(settings);

                    using (var form = new Form1(repo, cart, settings, showReceiptDialogs: false))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        using (new WinFormsTestHelper.DialogAutoCloser("Exit"))
                            WinFormsTestHelper.FindByName<Button>(form, "btnExit").PerformClick();

                        WinFormsTestHelper.PumpEvents();

                        Assert.IsFalse(form.Visible, "Form should close after confirming Exit with Yes.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void Form1_AboutButton_ShowsAboutDialog()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo     = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart     = new CartService(settings);

                    using (var form = new Form1(repo, cart, settings, showReceiptDialogs: false))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        using (new WinFormsTestHelper.DialogAutoCloser("About Pizza Express NZ"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "About").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void Form1_KeyboardHelp_ShowsAndCloses()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var repo     = new OrderRepository(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    var cart     = new CartService(settings);

                    using (var form = new Form1(repo, cart, settings, showReceiptDialogs: false))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        var mi = typeof(Form1).GetMethod(
                            "ShowKeyboardHelp",
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        Assert.IsNotNull(mi, "ShowKeyboardHelp method not found.");

                        using (new WinFormsTestHelper.DialogAutoCloser("Keyboard Shortcuts"))
                            mi.Invoke(form, null);

                        WinFormsTestHelper.PumpEvents();
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
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
