using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Config;
using WindowsFormsApplication3.Models;

namespace PizzaExpress.Tests
{
    [TestClass]
    public class OrderTests
    {
        private Order BuildOrder(params (string name, int qty, decimal price)[] lines)
        {
            var order = new Order();
            foreach (var (name, qty, price) in lines)
                order.Items.Add(new OrderItem(name, qty, price));
            return order;
        }

        [TestMethod]
        public void Subtotal_SingleItem_CorrectSum()
        {
            var order = BuildOrder(("Small Pizza", 1, 4.00m));
            Assert.AreEqual(4.00m, order.Subtotal);
        }

        [TestMethod]
        public void Subtotal_MultipleItems_CorrectSum()
        {
            // Large Pizza $10 + Pepperoni $0.75 + 2x Coke @ $2.90 each = $16.55
            var order = BuildOrder(
                ("Large Pizza",   1, 10.00m),
                ("  Pepperoni",   1,  0.75m),
                ("Coke - Can",    2,  2.90m));
            Assert.AreEqual(16.55m, order.Subtotal);
        }

        [TestMethod]
        public void Tax_IsSubtotalTimesGstRate()
        {
            var order = BuildOrder(("Medium Pizza", 1, 7.00m));
            decimal expected = System.Math.Round(7.00m * AppConfig.TaxRate, 2);
            Assert.AreEqual(expected, order.Tax);
        }

        [TestMethod]
        public void Total_IsSubtotalPlusTax()
        {
            var order = BuildOrder(("Extra Large Pizza", 1, 13.00m));
            Assert.AreEqual(order.Subtotal + order.Tax, order.Total);
        }

        [TestMethod]
        public void AmountDue_SubtractsDiscount()
        {
            var order = BuildOrder(("Extra Large Pizza", 1, 13.00m));
            order.Discount = 3.00m;

            Assert.AreEqual(order.Total - 3.00m, order.AmountDue);
        }

        [TestMethod]
        public void AmountDue_NeverDropsBelowZero()
        {
            var order = BuildOrder(("Small Pizza", 1, 4.00m));
            order.Discount = 999.00m;

            Assert.AreEqual(0m, order.AmountDue);
        }

        [TestMethod]
        public void Change_IsAmountPaidMinusTotal()
        {
            var order = BuildOrder(("Small Pizza", 1, 4.00m));
            order.AmountPaid = 10.00m;
            decimal expected = 10.00m - order.Total;
            Assert.AreEqual(expected, order.Change);
        }

        [TestMethod]
        public void Change_UsesDiscountedAmountDue()
        {
            var order = BuildOrder(("Large Pizza", 1, 10.00m));
            order.Discount = 2.50m;
            order.AmountPaid = order.AmountDue;

            Assert.AreEqual(0m, order.Change);
        }

        [TestMethod]
        public void EmptyOrder_SubtotalIsZero()
        {
            var order = new Order();
            Assert.AreEqual(0m, order.Subtotal);
        }

        [TestMethod]
        public void EmptyOrder_TaxIsZero()
        {
            var order = new Order();
            Assert.AreEqual(0m, order.Tax);
        }
    }
}
