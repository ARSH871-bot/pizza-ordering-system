using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    [TestClass]
    public class OrderSubmissionServiceTests
    {
        private static Order MakeOrder(string paymentMethod = "Cash", string reference = null)
        {
            return new Order
            {
                Customer = new Customer
                {
                    FirstName   = "Jane",
                    LastName    = "Doe",
                    Address     = "1 Queen St",
                    City        = "Auckland",
                    Region      = "Auckland",
                    PostalCode  = "1010",
                    ContactNo   = "0211234567",
                    Email       = "jane@example.com",
                },
                PaymentMethod    = paymentMethod,
                PaymentReference = reference,
                OrderDate        = new DateTime(2026, 5, 1, 10, 0, 0),
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_NullRepo_Throws()
        {
            _ = new OrderSubmissionService(null, Substitute.For<IReceiptWriter>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_NullReceiptWriter_Throws()
        {
            _ = new OrderSubmissionService(Substitute.For<IOrderRepository>(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Submit_NullOrder_Throws()
        {
            var svc = new OrderSubmissionService(
                Substitute.For<IOrderRepository>(),
                Substitute.For<IReceiptWriter>());
            svc.Submit(null);
        }

        [TestMethod]
        public void Submit_SavesRecordToRepository()
        {
            var repo    = Substitute.For<IOrderRepository>();
            var receipt = Substitute.For<IReceiptWriter>();
            receipt.Build(Arg.Any<Order>()).Returns("receipt text");

            var svc = new OrderSubmissionService(repo, receipt);
            svc.Submit(MakeOrder());

            repo.Received(1).Save(Arg.Any<OrderRecord>());
        }

        [TestMethod]
        public void Submit_ReturnsReceiptText()
        {
            var repo    = Substitute.For<IOrderRepository>();
            var receipt = Substitute.For<IReceiptWriter>();
            receipt.Build(Arg.Any<Order>()).Returns("my receipt");

            var svc    = new OrderSubmissionService(repo, receipt);
            var result = svc.Submit(MakeOrder());

            Assert.AreEqual("my receipt", result.ReceiptText);
        }

        [TestMethod]
        public void Submit_RecordStatusIsActive()
        {
            var repo    = Substitute.For<IOrderRepository>();
            var receipt = Substitute.For<IReceiptWriter>();
            receipt.Build(Arg.Any<Order>()).Returns("");

            var svc    = new OrderSubmissionService(repo, receipt);
            var result = svc.Submit(MakeOrder());

            Assert.AreEqual("Active", result.Record.Status);
        }

        [TestMethod]
        public void Submit_MapsCustomerFields()
        {
            var repo    = Substitute.For<IOrderRepository>();
            var receipt = Substitute.For<IReceiptWriter>();
            receipt.Build(Arg.Any<Order>()).Returns("");

            var svc    = new OrderSubmissionService(repo, receipt);
            var result = svc.Submit(MakeOrder());

            Assert.AreEqual("Jane Doe", result.Record.CustomerName);
            Assert.AreEqual("1 Queen St", result.Record.Address);
            Assert.AreEqual("Auckland",  result.Record.City);
            Assert.AreEqual("Auckland",  result.Record.Region);
            Assert.AreEqual("1010",      result.Record.PostalCode);
        }

        [TestMethod]
        public void Submit_MapsOrderLines()
        {
            var repo    = Substitute.For<IOrderRepository>();
            var receipt = Substitute.For<IReceiptWriter>();
            receipt.Build(Arg.Any<Order>()).Returns("");

            var order = MakeOrder();
            order.Items.Add(new OrderItem("Small Pizza", 2, 12.50m));
            order.Items.Add(new OrderItem("Coke - Can",  1,  3.00m));

            var svc    = new OrderSubmissionService(repo, receipt);
            var result = svc.Submit(order);

            Assert.AreEqual(2, result.Record.Lines.Count);
            Assert.AreEqual("Small Pizza", result.Record.Lines[0].Item);
            Assert.AreEqual(2,             result.Record.Lines[0].Quantity);
            Assert.AreEqual("Coke - Can",  result.Record.Lines[1].Item);
        }

        [TestMethod]
        public void Submit_CreditCard_PreservesPaymentReference()
        {
            var repo    = Substitute.For<IOrderRepository>();
            var receipt = Substitute.For<IReceiptWriter>();
            receipt.Build(Arg.Any<Order>()).Returns("");

            var order  = MakeOrder("Credit Card", "****5678");
            var svc    = new OrderSubmissionService(repo, receipt);
            var result = svc.Submit(order);

            Assert.AreEqual("****5678", result.Record.PaymentReference);
        }

        [TestMethod]
        public void Submit_Cash_PaymentReferenceIsNull()
        {
            var repo    = Substitute.For<IOrderRepository>();
            var receipt = Substitute.For<IReceiptWriter>();
            receipt.Build(Arg.Any<Order>()).Returns("");

            var order  = MakeOrder("Cash", null);
            var svc    = new OrderSubmissionService(repo, receipt);
            var result = svc.Submit(order);

            Assert.IsNull(result.Record.PaymentReference);
        }

        [TestMethod]
        public void Submit_RecordIdIs8CharUppercaseHex()
        {
            var repo    = Substitute.For<IOrderRepository>();
            var receipt = Substitute.For<IReceiptWriter>();
            receipt.Build(Arg.Any<Order>()).Returns("");

            var svc    = new OrderSubmissionService(repo, receipt);
            var result = svc.Submit(MakeOrder());

            Assert.AreEqual(8, result.Record.Id.Length);
            Assert.AreEqual(result.Record.Id, result.Record.Id.ToUpperInvariant());
        }
    }
}
