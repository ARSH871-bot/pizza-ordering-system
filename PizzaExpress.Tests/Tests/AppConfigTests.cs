using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Config;
using WindowsFormsApplication3.Models;

namespace PizzaExpress.Tests
{
    [TestClass]
    public class AppConfigTests
    {
        [TestMethod]
        public void TaxRate_IsNzGst_FifteenPercent()
            => Assert.AreEqual(0.15m, AppConfig.TaxRate);

        [TestMethod]
        public void CurrencyCode_IsNzd()
            => Assert.AreEqual("NZD", AppConfig.CurrencyCode);

        [TestMethod]
        public void TaxLabel_IsGst()
            => Assert.AreEqual("GST", AppConfig.TaxLabel);

        [TestMethod]
        public void PizzaPrices_SmallIs4()
            => Assert.AreEqual(4.00m, AppConfig.PizzaPrices[PizzaSize.Small]);

        [TestMethod]
        public void PizzaPrices_MediumIs7()
            => Assert.AreEqual(7.00m, AppConfig.PizzaPrices[PizzaSize.Medium]);

        [TestMethod]
        public void PizzaPrices_LargeIs10()
            => Assert.AreEqual(10.00m, AppConfig.PizzaPrices[PizzaSize.Large]);

        [TestMethod]
        public void PizzaPrices_ExtraLargeIs13()
            => Assert.AreEqual(13.00m, AppConfig.PizzaPrices[PizzaSize.ExtraLarge]);

        [TestMethod]
        public void NZRegions_Contains16Regions()
            => Assert.AreEqual(16, AppConfig.NZRegions.Count);

        [TestMethod]
        public void NZRegions_ContainsAuckland()
            => CollectionAssert.Contains((System.Collections.ICollection)AppConfig.NZRegions, "Auckland");

        [TestMethod]
        public void NZRegions_ContainsWellington()
            => CollectionAssert.Contains((System.Collections.ICollection)AppConfig.NZRegions, "Wellington");

        [TestMethod]
        public void PromoCodes_ContainsPizza10()
            => Assert.IsTrue(AppConfig.PromoCodes.ContainsKey("PIZZA10"));

        [TestMethod]
        public void PromoCodes_Pizza10Is10Percent()
            => Assert.AreEqual(0.10m, AppConfig.PromoCodes["PIZZA10"]);

        [TestMethod]
        public void PromoCodes_FreeshipIs100Percent()
            => Assert.AreEqual(1.00m, AppConfig.PromoCodes["FREESHIP"]);

        [TestMethod]
        public void PaymentMethods_ContainsCash()
            => CollectionAssert.Contains((System.Collections.ICollection)AppConfig.PaymentMethods, "Cash");

        [TestMethod]
        public void PaymentMethods_ContainsPromoCard()
            => CollectionAssert.Contains((System.Collections.ICollection)AppConfig.PaymentMethods, "Promo Card");
    }
}
