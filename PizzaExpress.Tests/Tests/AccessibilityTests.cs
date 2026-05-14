using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3;

namespace PizzaExpress.Tests.Tests
{
    [DoNotParallelize]
    [TestClass]
    public class AccessibilityTests
    {
        [TestMethod]
        public void Form1_ButtonMnemonics_ExpectedShortcutsAndNoDuplicates()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                var form = new Form1(showReceiptDialogs: false);
                form.Show();
                try
                {
                    // Verify exact expected text (catches wrong mnemonic position)
                    Assert.AreEqual("&Confirm Order",      form.ConfirmOrderButtonText,  "btnConfirmOrder");
                    Assert.AreEqual("C&heck Out",          form.CheckOutButtonText,       "btnCheckOut");
                    Assert.AreEqual("&Pay",                form.PayButtonText,            "btnPay");
                    Assert.AreEqual("&Submit Order",       form.SubmitOrderButtonText,    "btnSubmitOrder");

                    // Verify no two buttons share the same Alt shortcut character
                    var allTexts = new[]
                    {
                        form.ConfirmOrderButtonText,
                        form.CheckOutButtonText,
                        form.PayButtonText,
                        form.SubmitOrderButtonText,
                    };
                    var shortcuts = allTexts
                        .Select(t => { int i = t.IndexOf('&'); return i >= 0 ? char.ToUpper(t[i + 1]) : '\0'; })
                        .Where(c => c != '\0')
                        .ToList();
                    var duplicates = shortcuts.GroupBy(c => c).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                    Assert.AreEqual(0, duplicates.Count,
                        "Duplicate mnemonic characters: " + string.Join(", ", duplicates));
                }
                finally { form.Dispose(); }
            });
        }

        [TestMethod]
        public void Form1_AcceptButton_IsConfirmOrderOnTab0()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                var form = new Form1(showReceiptDialogs: false);
                form.Show();
                try
                {
                    Assert.IsNotNull(form.AcceptButton, "AcceptButton should be set");
                    Assert.AreEqual("&Confirm Order", ((Button)form.AcceptButton).Text);
                }
                finally { form.Dispose(); }
            });
        }

        [TestMethod]
        public void Form1_DecorativeImages_ExcludedFromAccessibilityTree()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                var form = new Form1(showReceiptDialogs: false);
                form.Show();
                try
                {
                    Assert.AreEqual(AccessibleRole.None, form.PictureBox1Role);
                    Assert.AreEqual(AccessibleRole.None, form.PictureBox2Role);
                }
                finally { form.Dispose(); }
            });
        }

        [TestMethod]
        public void Form1_InputAccessibleNames_AreDescriptive()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                var form = new Form1(showReceiptDialogs: false);
                form.Show();
                try
                {
                    Assert.AreEqual("First Name",            form.FirstNameAccessibleName);
                    Assert.AreEqual("Postal Code, 4 digits", form.PostalCodeAccessibleName);
                    Assert.AreEqual("Payment Method",        form.PaymentMethodAccessibleName);
                }
                finally { form.Dispose(); }
            });
        }
    }
}
