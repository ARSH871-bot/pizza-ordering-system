using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    /// <summary>
    /// Verifies that service interfaces are correctly implemented and that
    /// NSubstitute can substitute them (confirming the interfaces are non-sealed and mockable).
    /// </summary>
    [TestClass]
    public class ServiceInterfaceTests
    {
        // ── IPromoEngine ──────────────────────────────────────────────────────

        [TestMethod]
        public void IPromoEngine_MockReturnsConfiguredResult()
        {
            var engine = Substitute.For<IPromoEngine>();
            engine.Apply("TEST", 100m).Returns(new PromoResult
            {
                Success        = true,
                DiscountedTotal = 90m,
                Message        = "10% off",
            });

            PromoResult result = engine.Apply("TEST", 100m);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(90m, result.DiscountedTotal);
        }

        [TestMethod]
        public void IPromoEngine_RealImpl_ImplementsInterface()
        {
            IPromoEngine engine = new PromoEngine();
            Assert.IsNotNull(engine);
        }

        // ── IOrderValidator ───────────────────────────────────────────────────

        [TestMethod]
        public void IOrderValidator_MockReturnsConfiguredValidation()
        {
            var validator = Substitute.For<IOrderValidator>();
            validator.ValidatePostalCode("9999").Returns(ValidationResult.Ok());
            validator.ValidatePostalCode("bad").Returns(ValidationResult.Fail("Invalid"));

            Assert.IsTrue(validator.ValidatePostalCode("9999").IsValid);
            Assert.IsFalse(validator.ValidatePostalCode("bad").IsValid);
        }

        [TestMethod]
        public void IOrderValidator_RealImpl_ImplementsInterface()
        {
            IOrderValidator validator = new OrderValidator();
            Assert.IsNotNull(validator);
        }

        // ── IReceiptWriter ────────────────────────────────────────────────────

        [TestMethod]
        public void IReceiptWriter_MockCaptures_BuildCall()
        {
            var writer = Substitute.For<IReceiptWriter>();
            writer.Build(Arg.Any<Order>()).Returns("RECEIPT");

            var order  = new Order { Customer = new Customer { FirstName = "Test" } };
            string txt = writer.Build(order);

            Assert.AreEqual("RECEIPT", txt);
            writer.Received(1).Build(order);
        }

        [TestMethod]
        public void IReceiptWriter_RealImpl_ImplementsInterface()
        {
            IReceiptWriter writer = new ReceiptWriter();
            Assert.IsNotNull(writer);
        }

        // ── IOrderRepository ──────────────────────────────────────────────────

        [TestMethod]
        public void IOrderRepository_MockReturnsConfiguredRecords()
        {
            var repo   = Substitute.For<IOrderRepository>();
            var record = new OrderRecord { CustomerName = "Mock Customer" };
            repo.LoadAll().Returns(new List<OrderRecord> { record });

            List<OrderRecord> result = repo.LoadAll();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Mock Customer", result[0].CustomerName);
        }

        [TestMethod]
        public void IOrderRepository_MockVerifies_SaveWasCalled()
        {
            var repo   = Substitute.For<IOrderRepository>();
            var record = new OrderRecord { Id = "XYZ" };

            repo.Save(record);

            repo.Received(1).Save(record);
        }

        [TestMethod]
        public void IOrderRepository_RealImpl_ImplementsInterface()
        {
            IOrderRepository repo = new OrderRepository();
            Assert.IsNotNull(repo);
        }

        // ── ICartService ──────────────────────────────────────────────────────

        [TestMethod]
        public void ICartService_MockReturnsPizzaItems()
        {
            var cart = Substitute.For<ICartService>();
            cart.BuildPizzaItems(PizzaSize.Large, CrustType.Cheesy, 2, null)
                .Returns(new List<OrderItem> { new OrderItem("Test Pizza", 2, 10.00m) });

            var result = cart.BuildPizzaItems(PizzaSize.Large, CrustType.Cheesy, 2, null);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Test Pizza", result[0].Name);
        }

        [TestMethod]
        public void ICartService_MockVerifies_CalculateTotalCalled()
        {
            var cart = Substitute.For<ICartService>();
            cart.CalculateTotal(20.00m).Returns(23.00m);

            decimal result = cart.CalculateTotal(20.00m);

            Assert.AreEqual(23.00m, result);
            cart.Received(1).CalculateTotal(20.00m);
        }

        [TestMethod]
        public void ICartService_RealImpl_ImplementsInterface()
        {
            ICartService cart = new CartService();
            Assert.IsNotNull(cart);
        }

        // ── ILogger ───────────────────────────────────────────────────────────

        [TestMethod]
        public void ILogger_MockCapturesInfoCall()
        {
            var logger = Substitute.For<ILogger>();
            logger.Info("test message");
            logger.Received(1).Info("test message");
        }

        [TestMethod]
        public void ILogger_MockCapturesErrorWithException()
        {
            var logger = Substitute.For<ILogger>();
            var ex     = new InvalidOperationException("oops");
            logger.Error("something failed", ex);
            logger.Received(1).Error("something failed", ex);
        }

        [TestMethod]
        public void ILogger_NullLogger_ImplementsInterface()
        {
            ILogger logger = NullLogger.Instance;
            Assert.IsNotNull(logger);
        }

        [TestMethod]
        public void ILogger_FileLogger_ImplementsInterface()
        {
            ILogger logger = new FileLogger(System.IO.Path.GetTempPath());
            Assert.IsNotNull(logger);
        }
    }
}
