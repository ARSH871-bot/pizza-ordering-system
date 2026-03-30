using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests
{
    [TestClass]
    public class ReceiptWriterTests
    {
        private readonly ReceiptWriter _writer = new ReceiptWriter();

        private Order BuildSampleOrder()
        {
            var order = new Order
            {
                Customer = new Customer
                {
                    FirstName  = "Jane",
                    LastName   = "Doe",
                    Address    = "1 Queen St",
                    City       = "Auckland",
                    Region     = "Auckland",
                    PostalCode = "1010",
                    ContactNo  = "0211234567",
                },
                PaymentMethod = "Cash",
                AmountPaid    = 20.00m,
                OrderDate     = new DateTime(2026, 3, 24, 12, 0, 0),
            };
            order.Items.Add(new OrderItem("Normal Crust Small Pizza", 1, 4.00m));
            order.Items.Add(new OrderItem("  Pepperoni Toppings",     1, 0.75m));
            order.Items.Add(new OrderItem("Coke - Can",               2, 2.90m));
            return order;
        }

        [TestMethod]
        public void Build_ContainsStoreName()
            => StringAssert.Contains(_writer.Build(BuildSampleOrder()), "PIZZA EXPRESS");

        [TestMethod]
        public void Build_ContainsCustomerName()
            => StringAssert.Contains(_writer.Build(BuildSampleOrder()), "Jane Doe");

        [TestMethod]
        public void Build_ContainsAddress()
            => StringAssert.Contains(_writer.Build(BuildSampleOrder()), "1 Queen St");

        [TestMethod]
        public void Build_ContainsPostalCode()
            => StringAssert.Contains(_writer.Build(BuildSampleOrder()), "1010");

        [TestMethod]
        public void Build_ContainsOrderDate()
            => StringAssert.Contains(_writer.Build(BuildSampleOrder()), "2026-03-24");

        [TestMethod]
        public void Build_ContainsPizzaItem()
            => StringAssert.Contains(_writer.Build(BuildSampleOrder()), "Normal Crust Small Pizza");

        [TestMethod]
        public void Build_ContainsGstLabel()
            => StringAssert.Contains(_writer.Build(BuildSampleOrder()), "GST");

        [TestMethod]
        public void Build_ContainsCurrencyCode()
            => StringAssert.Contains(_writer.Build(BuildSampleOrder()), "NZD");

        [TestMethod]
        public void Build_ContainsPaymentMethod()
            => StringAssert.Contains(_writer.Build(BuildSampleOrder()), "Cash");

        [TestMethod]
        public void Build_ContainsThankyouMessage()
            => StringAssert.Contains(_writer.Build(BuildSampleOrder()), "Thank you");

        [TestMethod]
        public void Build_NonNullAndNonEmpty()
        {
            string receipt = _writer.Build(BuildSampleOrder());
            Assert.IsNotNull(receipt);
            Assert.IsTrue(receipt.Length > 0);
        }

        [TestMethod]
        public void Build_ContainsSubtotalLine()
            => StringAssert.Contains(_writer.Build(BuildSampleOrder()), "Subtotal:");

        [TestMethod]
        public void Build_ContainsChangeLine()
            => StringAssert.Contains(_writer.Build(BuildSampleOrder()), "Change:");

        [TestMethod]
        public void Build_ContainsContactNo()
            => StringAssert.Contains(_writer.Build(BuildSampleOrder()), "0211234567");

        [TestMethod]
        public void Build_ContainsRegion()
            => StringAssert.Contains(_writer.Build(BuildSampleOrder()), "Auckland");

        [TestMethod]
        public void Build_ContainsDeliveryTime()
            => StringAssert.Contains(_writer.Build(BuildSampleOrder()), "30");

        [TestMethod]
        public void Build_WithDiscount_IncludesDiscountLineAndDiscountedTotal()
        {
            var order = BuildSampleOrder();
            order.Discount = 2.00m;
            order.DiscountDescription = "PROMO10";

            string receipt = _writer.Build(order);

            StringAssert.Contains(receipt, "Discount (PROMO10):");
            StringAssert.Contains(receipt, order.AmountDue.ToString("C2"));
        }

        [TestMethod]
        public void Build_UsesOrderDeliveryMinutes()
        {
            var order = BuildSampleOrder();
            order.DeliveryMinutes = 45;

            string receipt = _writer.Build(order);

            StringAssert.Contains(receipt, "45");
        }

        [TestMethod]
        public void SaveToFile_WritesContentToDisk()
        {
            string path = System.IO.Path.GetTempFileName();
            try
            {
                _writer.SaveToFile(BuildSampleOrder(), path);
                string content = System.IO.File.ReadAllText(path);
                StringAssert.Contains(content, "PIZZA EXPRESS");
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        // Regression: BuildOrderForReceipt stores line totals in the ListView price column.
        // Unit price must be back-calculated as lineTotal / max(qty,1) before constructing
        // OrderItem, otherwise TotalPrice = unitPrice × qty double-counts quantity.
        [TestMethod]
        public void Build_MultiQtyDrink_SubtotalNotDoubledByQuantity()
        {
            // Coke x2 @ $1.45/can → line total $2.90 → unit price must be $1.45
            var order = new Order
            {
                Customer      = new Customer { FirstName = "Test", LastName = "User",
                    Region = "Auckland", PostalCode = "1010" },
                PaymentMethod = "Cash",
                AmountPaid    = 50.00m,
            };
            order.Items.Add(new OrderItem("Coke - Can", 2, 1.45m));   // correct unit price
            string receipt = _writer.Build(order);

            // Subtotal = 2 × $1.45 = $2.90 (not $5.80 which was the bug)
            StringAssert.Contains(receipt, "2.90");
        }

        [TestMethod]
        public void Build_MultiQtyDrink_DoesNotDoubleCount()
        {
            // If the buggy price ($2.90 line total used as unit price) were passed in,
            // TotalPrice would be $5.80 — verify the receipt does NOT contain 5.80
            // when the correct unit price $1.45 is supplied.
            var order = new Order
            {
                Customer      = new Customer { FirstName = "Test", LastName = "User",
                    Region = "Auckland", PostalCode = "1010" },
                PaymentMethod = "Cash",
                AmountPaid    = 50.00m,
            };
            order.Items.Add(new OrderItem("Coke - Can", 2, 1.45m));
            string receipt = _writer.Build(order);

            Assert.IsFalse(receipt.Contains("5.80"),
                "Subtotal should be $2.90 (unit $1.45 × qty 2), not $5.80 (line-total double-counted).");
        }
    }
}
