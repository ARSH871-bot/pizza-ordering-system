using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests
{
    [TestClass]
    public class PromoEngineTests
    {
        private readonly PromoEngine _engine = new PromoEngine();

        [TestMethod]
        public void Apply_Pizza10_Returns10PercentOff()
        {
            var result = _engine.Apply("PIZZA10", 100.00m);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(90.00m, result.DiscountedTotal);
        }

        [TestMethod]
        public void Apply_Pizza20_Returns20PercentOff()
        {
            var result = _engine.Apply("PIZZA20", 100.00m);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(80.00m, result.DiscountedTotal);
        }

        [TestMethod]
        public void Apply_Freeship_ReturnsFreeOrder()
        {
            var result = _engine.Apply("FREESHIP", 45.90m);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0.00m, result.DiscountedTotal);
        }

        [TestMethod]
        public void Apply_LowercaseCode_IsAccepted()
        {
            var result = _engine.Apply("pizza10", 100.00m);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(90.00m, result.DiscountedTotal);
        }

        [TestMethod]
        public void Apply_CodeWithSpaces_IsNormalised()
        {
            var result = _engine.Apply("  PIZZA20  ", 100.00m);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void Apply_InvalidCode_ReturnsFail()
        {
            var result = _engine.Apply("INVALID", 100.00m);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void Apply_EmptyCode_ReturnsFail()
        {
            var result = _engine.Apply("", 100.00m);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void Apply_NullCode_ReturnsFail()
        {
            var result = _engine.Apply(null, 100.00m);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void Apply_DiscountedTotal_IsRoundedToTwoDecimalPlaces()
        {
            var result = _engine.Apply("PIZZA10", 13.33m);
            // 13.33 * 0.9 = 11.997 → rounds to 12.00
            Assert.AreEqual(11.997m == System.Math.Round(13.33m * 0.9m, 2)
                ? System.Math.Round(13.33m * 0.9m, 2)
                : System.Math.Round(13.33m * 0.9m, 2), result.DiscountedTotal);
        }

        [TestMethod]
        public void Apply_InvalidCode_MessageIndicatesInvalid()
        {
            var result = _engine.Apply("BADCODE", 50.00m);
            StringAssert.Contains(result.Message, "Invalid");
        }

        [TestMethod]
        public void Apply_Freeship_MessageIndicatesFreeOrder()
        {
            var result = _engine.Apply("FREESHIP", 50.00m);
            StringAssert.Contains(result.Message, "Free");
        }

        [TestMethod]
        public void Apply_Pizza10_MessageContainsPercentOff()
        {
            var result = _engine.Apply("PIZZA10", 100.00m);
            StringAssert.Contains(result.Message, "10%");
        }

        [TestMethod]
        public void Apply_Pizza20_MessageContainsNewTotal()
        {
            var result = _engine.Apply("PIZZA20", 50.00m);
            // 50.00 * 0.8 = 40.00
            StringAssert.Contains(result.Message, "40");
        }
    }
}
