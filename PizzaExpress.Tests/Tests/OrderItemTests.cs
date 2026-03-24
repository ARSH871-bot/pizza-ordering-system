using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Models;

namespace PizzaExpress.Tests
{
    [TestClass]
    public class OrderItemTests
    {
        [TestMethod]
        public void TotalPrice_SingleItem_EqualUnitPrice()
        {
            var item = new OrderItem("Small Pizza", 1, 4.00m);
            Assert.AreEqual(4.00m, item.TotalPrice);
        }

        [TestMethod]
        public void TotalPrice_MultipleItems_MultipliesCorrectly()
        {
            var item = new OrderItem("Large Pizza", 3, 10.00m);
            Assert.AreEqual(30.00m, item.TotalPrice);
        }

        [TestMethod]
        public void TotalPrice_ZeroQuantity_TreatedAsOne()
        {
            var item = new OrderItem("Topping", 0, 0.75m);
            Assert.AreEqual(0.75m, item.TotalPrice);
        }

        [TestMethod]
        public void TotalPrice_IsRoundedToTwoDecimalPlaces()
        {
            var item = new OrderItem("Drink", 3, 1.45m);
            Assert.AreEqual(4.35m, item.TotalPrice);
        }

        [TestMethod]
        public void Constructor_SetsAllProperties()
        {
            var item = new OrderItem("Test Item", 2, 5.50m);
            Assert.AreEqual("Test Item", item.Name);
            Assert.AreEqual(2,     item.Quantity);
            Assert.AreEqual(5.50m, item.UnitPrice);
        }
    }
}
