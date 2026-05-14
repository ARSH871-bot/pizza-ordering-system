using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    [TestClass]
    public class CheckoutWorkflowServiceTests
    {
        private static CheckoutWorkflowService MakeService()
            => new CheckoutWorkflowService(new PromoEngine(), new OrderValidator());

        private static List<OrderItem> OneSmallPizza()
            => new List<OrderItem> { new OrderItem("Small Pizza", 1, 4.00m) };

        // ── BuildCustomer ─────────────────────────────────────────────────────

        [TestMethod]
        public void BuildCustomer_SetsAllFields()
        {
            var svc = MakeService();
            var c = svc.BuildCustomer("Jane", "Doe", "1 Main St", "Wellington", "Wellington", "6011", "0211234567", "j@d.com");

            Assert.AreEqual("Jane",         c.FirstName);
            Assert.AreEqual("Doe",          c.LastName);
            Assert.AreEqual("1 Main St",    c.Address);
            Assert.AreEqual("Wellington",   c.City);
            Assert.AreEqual("Wellington",   c.Region);
            Assert.AreEqual("6011",         c.PostalCode);
            Assert.AreEqual("0211234567",   c.ContactNo);
            Assert.AreEqual("j@d.com",      c.Email);
        }

        // ── ValidateCustomer ──────────────────────────────────────────────────

        [TestMethod]
        public void ValidateCustomer_MissingFirstName_Fails()
        {
            var svc = MakeService();
            var c   = new Customer { LastName = "Doe", Address = "1 Main St", PostalCode = "6011" };
            Assert.IsFalse(svc.ValidateCustomer(c).IsValid);
        }

        [TestMethod]
        public void ValidateCustomer_ValidCustomer_Passes()
        {
            var svc = MakeService();
            var c   = new Customer
            {
                FirstName  = "Jane", LastName = "Doe",
                Address    = "1 Main St", PostalCode = "6011",
            };
            Assert.IsTrue(svc.ValidateCustomer(c).IsValid);
        }

        // ── ApplyPromo ────────────────────────────────────────────────────────

        [TestMethod]
        public void ApplyPromo_ValidCode_ReturnsSuccess()
        {
            var svc    = MakeService();
            var result = svc.ApplyPromo("PIZZA10", 100m);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(90m,      result.DiscountedTotal);
            Assert.AreEqual("PIZZA10", result.AppliedCode);
            StringAssert.Contains(result.Message, "10%");
        }

        [TestMethod]
        public void ApplyPromo_InvalidCode_ReturnsFail()
        {
            var svc    = MakeService();
            var result = svc.ApplyPromo("NOTREAL", 100m);

            Assert.IsFalse(result.Success);
            Assert.IsNull(result.AppliedCode);
        }

        [TestMethod]
        public void ApplyPromo_FreeshipCode_FullDiscount()
        {
            var svc    = MakeService();
            var result = svc.ApplyPromo("FREESHIP", 50m);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0m, result.DiscountedTotal);
        }

        // ── ProcessStandardPayment ────────────────────────────────────────────

        [TestMethod]
        public void ProcessStandardPayment_Cash_ExactAmount_Succeeds()
        {
            var svc    = MakeService();
            var result = svc.ProcessStandardPayment("Cash", 20m, 20m);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0m, result.Change);
        }

        [TestMethod]
        public void ProcessStandardPayment_Cash_Overpayment_ReturnsChange()
        {
            var svc    = MakeService();
            var result = svc.ProcessStandardPayment("Cash", 25m, 20m);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(5m, result.Change);
        }

        [TestMethod]
        public void ProcessStandardPayment_Underpayment_Fails()
        {
            var svc    = MakeService();
            var result = svc.ProcessStandardPayment("Cash", 10m, 20m);

            Assert.IsFalse(result.Success);
            StringAssert.Contains(result.ErrorMessage, "less than");
        }

        [TestMethod]
        public void ProcessStandardPayment_EmptyMethod_Fails()
        {
            var svc    = MakeService();
            var result = svc.ProcessStandardPayment("", 20m, 20m);

            Assert.IsFalse(result.Success);
        }

        // ── AssembleOrder ─────────────────────────────────────────────────────

        [TestMethod]
        public void AssembleOrder_CardReference_IsMasked()
        {
            var svc      = MakeService();
            var customer = new Customer { FirstName = "Jane", LastName = "Doe" };
            var order    = svc.AssembleOrder(customer, "Credit Card", "4111111111111111",
                null, 20m, 20m, 20m, 30, OneSmallPizza());

            Assert.AreEqual("****1111", order.PaymentReference);
        }

        [TestMethod]
        public void AssembleOrder_Cash_NoPaymentReference()
        {
            var svc      = MakeService();
            var customer = new Customer { FirstName = "Jane", LastName = "Doe" };
            var order    = svc.AssembleOrder(customer, "Cash", "",
                null, 20m, 20m, 20m, 30, OneSmallPizza());

            Assert.IsNull(order.PaymentReference);
        }

        [TestMethod]
        public void AssembleOrder_WithPromo_SetsDiscountDescription()
        {
            var svc      = MakeService();
            var customer = new Customer { FirstName = "Jane", LastName = "Doe" };
            var order    = svc.AssembleOrder(customer, "Promo Card", "PIZZA20",
                "PIZZA20", 100m, 80m, 80m, 30, OneSmallPizza());

            Assert.AreEqual("PIZZA20", order.DiscountDescription);
            Assert.AreEqual(20m,       order.Discount);
        }

        // ── ParseCurrencyOrZero ───────────────────────────────────────────────

        [TestMethod]
        public void ParseCurrencyOrZero_NzdString_ParsesCorrectly()
        {
            decimal result = CheckoutWorkflowService.ParseCurrencyOrZero("$12.50");
            Assert.AreEqual(12.50m, result);
        }

        [TestMethod]
        public void ParseCurrencyOrZero_EmptyString_ReturnsZero()
        {
            decimal result = CheckoutWorkflowService.ParseCurrencyOrZero("");
            Assert.AreEqual(0m, result);
        }

        // ── GetDeliveryMinutes ────────────────────────────────────────────────

        [TestMethod]
        public void GetDeliveryMinutes_NullSettings_ReturnsDefault()
        {
            var svc = MakeService();
            Assert.AreEqual(30, svc.GetDeliveryMinutes(null));
        }
    }
}
