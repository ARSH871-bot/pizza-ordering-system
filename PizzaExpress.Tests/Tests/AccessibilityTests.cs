using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3;
using WindowsFormsApplication3.Infrastructure;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    [DoNotParallelize]
    [TestClass]
    public class AccessibilityTests
    {
        [TestMethod]
        public void Form1_ButtonMnemonics_AreSetOnLoad()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                var form = new Form1(showReceiptDialogs: false);
                form.Show();
                try
                {
                    Assert.IsTrue(form.ConfirmOrderButtonText.Contains("&"), "btnConfirmOrder must have a mnemonic");
                    Assert.IsTrue(form.CheckOutButtonText.Contains("&"), "btnCheckOut must have a mnemonic");
                    Assert.IsTrue(form.PayButtonText.Contains("&"), "btnPay must have a mnemonic");
                    Assert.IsTrue(form.SubmitOrderButtonText.Contains("&"), "btnSubmitOrder must have a mnemonic");
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
