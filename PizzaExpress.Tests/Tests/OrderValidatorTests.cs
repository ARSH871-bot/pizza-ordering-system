using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests
{
    [TestClass]
    public class OrderValidatorTests
    {
        private readonly OrderValidator _validator = new OrderValidator();

        // ── Postal code ───────────────────────────────────────────────────────

        [TestMethod]
        public void PostalCode_Valid4Digits_IsOk()
            => Assert.IsTrue(_validator.ValidatePostalCode("1010").IsValid);

        [TestMethod]
        public void PostalCode_Valid4Digits_ZeroPrefix_IsOk()
            => Assert.IsTrue(_validator.ValidatePostalCode("0110").IsValid);

        [TestMethod]
        public void PostalCode_ThreeDigits_Fails()
            => Assert.IsFalse(_validator.ValidatePostalCode("101").IsValid);

        [TestMethod]
        public void PostalCode_FiveDigits_Fails()
            => Assert.IsFalse(_validator.ValidatePostalCode("10101").IsValid);

        [TestMethod]
        public void PostalCode_Letters_Fails()
            => Assert.IsFalse(_validator.ValidatePostalCode("AB1C").IsValid);

        [TestMethod]
        public void PostalCode_Empty_Fails()
            => Assert.IsFalse(_validator.ValidatePostalCode("").IsValid);

        [TestMethod]
        public void PostalCode_Null_Fails()
            => Assert.IsFalse(_validator.ValidatePostalCode(null).IsValid);

        // ── Contact number ────────────────────────────────────────────────────

        [TestMethod]
        public void ContactNo_Empty_IsOk_BecauseOptional()
            => Assert.IsTrue(_validator.ValidateContactNo("").IsValid);

        [TestMethod]
        public void ContactNo_Null_IsOk_BecauseOptional()
            => Assert.IsTrue(_validator.ValidateContactNo(null).IsValid);

        [TestMethod]
        public void ContactNo_ValidNzMobile_IsOk()
            => Assert.IsTrue(_validator.ValidateContactNo("0211234567").IsValid);

        [TestMethod]
        public void ContactNo_ValidWithPlusPrefix_IsOk()
            => Assert.IsTrue(_validator.ValidateContactNo("+6421123456").IsValid);

        [TestMethod]
        public void ContactNo_TooShort_Fails()
            => Assert.IsFalse(_validator.ValidateContactNo("12345").IsValid);

        [TestMethod]
        public void ContactNo_WithLetters_Fails()
            => Assert.IsFalse(_validator.ValidateContactNo("021abc1234").IsValid);

        // ── Customer aggregate ────────────────────────────────────────────────

        [TestMethod]
        public void ValidateCustomer_AllRequiredFields_IsOk()
        {
            var c = new Customer
            {
                FirstName = "Jane", LastName = "Doe",
                Address = "1 Queen St", PostalCode = "1010"
            };
            Assert.IsTrue(_validator.ValidateCustomer(c).IsValid);
        }

        [TestMethod]
        public void ValidateCustomer_MissingFirstName_Fails()
        {
            var c = new Customer { LastName = "Doe", Address = "1 Queen St", PostalCode = "1010" };
            Assert.IsFalse(_validator.ValidateCustomer(c).IsValid);
        }

        [TestMethod]
        public void ValidateCustomer_MissingPostalCode_Fails()
        {
            var c = new Customer { FirstName = "Jane", LastName = "Doe", Address = "1 Queen St" };
            Assert.IsFalse(_validator.ValidateCustomer(c).IsValid);
        }

        [TestMethod]
        public void ValidateCustomer_InvalidPostalCode_Fails()
        {
            var c = new Customer
            {
                FirstName = "Jane", LastName = "Doe",
                Address = "1 Queen St", PostalCode = "ABCD"
            };
            Assert.IsFalse(_validator.ValidateCustomer(c).IsValid);
        }

        // ── Payment ───────────────────────────────────────────────────────────

        [TestMethod]
        public void ValidatePayment_ExactAmount_IsOk()
            => Assert.IsTrue(_validator.ValidatePayment("Cash", 20.00m, 20.00m).IsValid);

        [TestMethod]
        public void ValidatePayment_Overpayment_IsOk()
            => Assert.IsTrue(_validator.ValidatePayment("Cash", 50.00m, 20.00m).IsValid);

        [TestMethod]
        public void ValidatePayment_Underpayment_Fails()
            => Assert.IsFalse(_validator.ValidatePayment("Cash", 10.00m, 20.00m).IsValid);

        [TestMethod]
        public void ValidatePayment_NoMethod_Fails()
            => Assert.IsFalse(_validator.ValidatePayment("", 20.00m, 20.00m).IsValid);

        // ── Order items ───────────────────────────────────────────────────────

        [TestMethod]
        public void ValidateOrder_WithPizza_IsOk()
        {
            var items = new List<OrderItem> { new OrderItem("Normal Crust Small Pizza", 1, 4.00m) };
            Assert.IsTrue(_validator.ValidateOrder(items).IsValid);
        }

        [TestMethod]
        public void ValidateOrder_EmptyList_Fails()
            => Assert.IsFalse(_validator.ValidateOrder(new List<OrderItem>()).IsValid);

        [TestMethod]
        public void ValidateOrder_NullList_Fails()
            => Assert.IsFalse(_validator.ValidateOrder(null).IsValid);

        [TestMethod]
        public void ValidateOrder_OnlyDrinks_NoPizza_Fails()
        {
            var items = new List<OrderItem> { new OrderItem("Coke - Can", 2, 2.90m) };
            Assert.IsFalse(_validator.ValidateOrder(items).IsValid);
        }
    }
}
