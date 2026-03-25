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
    }
}
