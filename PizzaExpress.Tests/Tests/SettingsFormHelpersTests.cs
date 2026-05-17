using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Forms;

namespace PizzaExpress.Tests.Tests
{
    [TestClass]
    public class SettingsFormHelpersTests
    {
        // ── FriendlyName ──────────────────────────────────────────────────────

        [TestMethod]
        public void FriendlyName_PizzaPriceSmall_ReturnsCorrectLabel()
            => StringAssert.Contains(SettingsForm.FriendlyName("PizzaPrice.Small"), "Small");

        [TestMethod]
        public void FriendlyName_PizzaPriceMedium_ReturnsCorrectLabel()
            => StringAssert.Contains(SettingsForm.FriendlyName("PizzaPrice.Medium"), "Medium");

        [TestMethod]
        public void FriendlyName_PizzaPriceLarge_ReturnsCorrectLabel()
            => StringAssert.Contains(SettingsForm.FriendlyName("PizzaPrice.Large"), "Large");

        [TestMethod]
        public void FriendlyName_PizzaPriceExtraLarge_ReturnsCorrectLabel()
            => StringAssert.Contains(SettingsForm.FriendlyName("PizzaPrice.ExtraLarge"), "Extra Large");

        [TestMethod]
        public void FriendlyName_ToppingPrice_ReturnsCorrectLabel()
            => StringAssert.Contains(SettingsForm.FriendlyName("ToppingPrice"), "Topping");

        [TestMethod]
        public void FriendlyName_DrinkCanPrice_ReturnsCorrectLabel()
            => StringAssert.Contains(SettingsForm.FriendlyName("DrinkCanPrice"), "Drink");

        [TestMethod]
        public void FriendlyName_WaterPrice_ReturnsCorrectLabel()
            => StringAssert.Contains(SettingsForm.FriendlyName("WaterPrice"), "Water");

        [TestMethod]
        public void FriendlyName_SidePrice_ReturnsCorrectLabel()
            => StringAssert.Contains(SettingsForm.FriendlyName("SidePrice"), "Side");

        [TestMethod]
        public void FriendlyName_DeliveryMinutes_ReturnsCorrectLabel()
            => StringAssert.Contains(SettingsForm.FriendlyName("DeliveryMinutes"), "Delivery");

        [TestMethod]
        public void FriendlyName_StaffPin_ReturnsCorrectLabel()
            => StringAssert.Contains(SettingsForm.FriendlyName("StaffPin"), "PIN");

        [TestMethod]
        public void FriendlyName_UnknownKey_ReturnsKeyAsIs()
            => Assert.AreEqual("UnknownKey", SettingsForm.FriendlyName("UnknownKey"));

        [TestMethod]
        public void FriendlyName_AllKnownKeys_ReturnNonEmptyStrings()
        {
            string[] keys =
            {
                "PizzaPrice.Small", "PizzaPrice.Medium", "PizzaPrice.Large",
                "PizzaPrice.ExtraLarge", "ToppingPrice", "DrinkCanPrice",
                "WaterPrice", "SidePrice", "DeliveryMinutes", "StaffPin",
            };
            foreach (string key in keys)
                Assert.IsFalse(string.IsNullOrWhiteSpace(SettingsForm.FriendlyName(key)),
                    $"FriendlyName(\"{key}\") should return a non-empty label");
        }

        // ── IsNumericKey ──────────────────────────────────────────────────────

        [TestMethod]
        public void IsNumericKey_PizzaPriceSmall_ReturnsTrue()
            => Assert.IsTrue(SettingsForm.IsNumericKey("PizzaPrice.Small"));

        [TestMethod]
        public void IsNumericKey_PizzaPriceMedium_ReturnsTrue()
            => Assert.IsTrue(SettingsForm.IsNumericKey("PizzaPrice.Medium"));

        [TestMethod]
        public void IsNumericKey_PizzaPriceLarge_ReturnsTrue()
            => Assert.IsTrue(SettingsForm.IsNumericKey("PizzaPrice.Large"));

        [TestMethod]
        public void IsNumericKey_PizzaPriceExtraLarge_ReturnsTrue()
            => Assert.IsTrue(SettingsForm.IsNumericKey("PizzaPrice.ExtraLarge"));

        [TestMethod]
        public void IsNumericKey_ToppingPrice_ReturnsTrue()
            => Assert.IsTrue(SettingsForm.IsNumericKey("ToppingPrice"));

        [TestMethod]
        public void IsNumericKey_DrinkCanPrice_ReturnsTrue()
            => Assert.IsTrue(SettingsForm.IsNumericKey("DrinkCanPrice"));

        [TestMethod]
        public void IsNumericKey_WaterPrice_ReturnsTrue()
            => Assert.IsTrue(SettingsForm.IsNumericKey("WaterPrice"));

        [TestMethod]
        public void IsNumericKey_SidePrice_ReturnsTrue()
            => Assert.IsTrue(SettingsForm.IsNumericKey("SidePrice"));

        [TestMethod]
        public void IsNumericKey_DeliveryMinutes_ReturnsTrue()
            => Assert.IsTrue(SettingsForm.IsNumericKey("DeliveryMinutes"));

        [TestMethod]
        public void IsNumericKey_StaffPin_ReturnsFalse()
            => Assert.IsFalse(SettingsForm.IsNumericKey("StaffPin"));

        [TestMethod]
        public void IsNumericKey_UnknownKey_ReturnsFalse()
            => Assert.IsFalse(SettingsForm.IsNumericKey("UnknownRandomKey"));

        [TestMethod]
        public void IsNumericKey_Empty_ReturnsFalse()
            => Assert.IsFalse(SettingsForm.IsNumericKey(""));
    }
}
