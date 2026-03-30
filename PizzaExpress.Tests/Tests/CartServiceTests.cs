using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Models;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    [TestClass]
    public class CartServiceTests
    {
        private readonly CartService _cart = new CartService();

        // ── BuildPizzaItems ───────────────────────────────────────────────────

        [TestMethod]
        public void BuildPizzaItems_SmallNormal_NoToppings_ReturnsSingleItem()
        {
            var items = _cart.BuildPizzaItems(PizzaSize.Small, CrustType.Normal, 1, null);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual("Normal Crust Small Pizza", items[0].Name);
            Assert.AreEqual(4.00m, items[0].TotalPrice);
        }

        [TestMethod]
        public void BuildPizzaItems_Qty3_PriceScales()
        {
            var items = _cart.BuildPizzaItems(PizzaSize.Medium, CrustType.Cheesy, 3, null);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(21.00m, items[0].TotalPrice); // 7.00 × 3
        }

        [TestMethod]
        public void BuildPizzaItems_ExtraLarge_NameIsCorrect()
        {
            var items = _cart.BuildPizzaItems(PizzaSize.ExtraLarge, CrustType.Sausage, 1, null);
            Assert.AreEqual("Sausage Crust Extra Large Pizza", items[0].Name);
        }

        [TestMethod]
        public void BuildPizzaItems_TwoToppings_ReturnsThreeItems()
        {
            var toppings = new List<string> { "Pepperoni", "Mushroom" };
            var items    = _cart.BuildPizzaItems(PizzaSize.Large, CrustType.Normal, 1, toppings);
            Assert.AreEqual(3, items.Count);  // 1 pizza + 2 toppings
        }

        [TestMethod]
        public void BuildPizzaItems_ToppingPrice_IsCorrect()
        {
            var toppings = new List<string> { "Bacon" };
            var items    = _cart.BuildPizzaItems(PizzaSize.Small, CrustType.Normal, 1, toppings);
            Assert.AreEqual(0.75m, items[1].TotalPrice);
        }

        [TestMethod]
        public void BuildPizzaItems_ToppingName_HasIndentPrefix()
        {
            var items = _cart.BuildPizzaItems(PizzaSize.Small, CrustType.Normal, 1,
                new List<string> { "Pineapple" });
            Assert.AreEqual("  Pineapple Toppings", items[1].Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BuildPizzaItems_ZeroQty_Throws()
            => _cart.BuildPizzaItems(PizzaSize.Small, CrustType.Normal, 0, null);

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BuildPizzaItems_NegativeQty_Throws()
            => _cart.BuildPizzaItems(PizzaSize.Small, CrustType.Normal, -1, null);

        [TestMethod]
        public void BuildPizzaItems_EmptyToppingName_Skipped()
        {
            var toppings = new List<string> { "", "  ", "Spinach" };
            var items    = _cart.BuildPizzaItems(PizzaSize.Small, CrustType.Normal, 1, toppings);
            Assert.AreEqual(2, items.Count);  // pizza + Spinach only
        }

        [TestMethod]
        public void BuildPizzaItems_LargePizza_PriceIsCorrect()
        {
            var items = _cart.BuildPizzaItems(PizzaSize.Large, CrustType.Normal, 1, null);
            Assert.AreEqual(10.00m, items[0].TotalPrice); // AppConfig.PizzaPrices[Large] = 10.00
        }

        // ── CalculateSubtotal ─────────────────────────────────────────────────

        [TestMethod]
        public void CalculateSubtotal_EmptyList_ReturnsZero()
            => Assert.AreEqual(0m, _cart.CalculateSubtotal(new List<OrderItem>()));

        [TestMethod]
        public void CalculateSubtotal_Null_ReturnsZero()
            => Assert.AreEqual(0m, _cart.CalculateSubtotal(null));

        [TestMethod]
        public void CalculateSubtotal_MultipleItems_SumsCorrectly()
        {
            var items = new List<OrderItem>
            {
                new OrderItem("Pizza", 1, 10.00m),
                new OrderItem("Coke",  2,  1.45m),
            };
            Assert.AreEqual(12.90m, _cart.CalculateSubtotal(items));
        }

        // ── CalculateTax ──────────────────────────────────────────────────────

        [TestMethod]
        public void CalculateTax_Zero_ReturnsZero()
            => Assert.AreEqual(0m, _cart.CalculateTax(0m));

        [TestMethod]
        public void CalculateTax_TenDollars_ReturnsFifteenPercent()
            => Assert.AreEqual(1.50m, _cart.CalculateTax(10.00m));

        [TestMethod]
        public void CalculateTax_RoundedToTwoDecimalPlaces()
            => Assert.AreEqual(0.11m, _cart.CalculateTax(0.73m)); // 0.73 * 0.15 = 0.1095 → 0.11

        // ── CalculateTotal ────────────────────────────────────────────────────

        [TestMethod]
        public void CalculateTotal_TenDollars_ReturnsElevenFifty()
            => Assert.AreEqual(11.50m, _cart.CalculateTotal(10.00m));

        [TestMethod]
        public void CalculateTotal_Zero_ReturnsZero()
            => Assert.AreEqual(0m, _cart.CalculateTotal(0m));

        [TestMethod]
        public void BuildPizzaItems_WithSettingsRepository_UsesConfiguredPrices()
        {
            var settings = new StubSettingsRepository(new Dictionary<string, string>
            {
                ["PizzaPrice.Small"] = "5.50",
                ["ToppingPrice"] = "1.25",
            });
            var cart = new CartService(settings);

            var items = cart.BuildPizzaItems(
                PizzaSize.Small,
                CrustType.Normal,
                2,
                new List<string> { "Pepperoni" });

            Assert.AreEqual(11.00m, items[0].TotalPrice);
            Assert.AreEqual(1.25m, items[1].TotalPrice);
        }

        [TestMethod]
        public void GetDrinkCanPrice_UsesConfiguredSetting()
        {
            var cart = new CartService(new StubSettingsRepository(
                new Dictionary<string, string> { ["DrinkCanPrice"] = "1.95" }));

            Assert.AreEqual(1.95m, cart.GetDrinkCanPrice());
        }

        [TestMethod]
        public void GetWaterPrice_UsesConfiguredSetting()
        {
            var cart = new CartService(new StubSettingsRepository(
                new Dictionary<string, string> { ["WaterPrice"] = "1.60" }));

            Assert.AreEqual(1.60m, cart.GetWaterPrice());
        }

        [TestMethod]
        public void GetSidePrice_UsesConfiguredSetting()
        {
            var cart = new CartService(new StubSettingsRepository(
                new Dictionary<string, string> { ["SidePrice"] = "4.25" }));

            Assert.AreEqual(4.25m, cart.GetSidePrice());
        }

        private sealed class StubSettingsRepository : ISettingsRepository
        {
            private readonly Dictionary<string, string> _values;

            public StubSettingsRepository(Dictionary<string, string> values)
            {
                _values = values ?? new Dictionary<string, string>();
            }

            public string Get(string key, string defaultValue = null)
            {
                return _values.TryGetValue(key, out var value) ? value : defaultValue;
            }

            public void Set(string key, string value)
            {
                _values[key] = value;
            }

            public IReadOnlyList<SettingRow> GetAll()
            {
                var rows = new List<SettingRow>();
                foreach (var kvp in _values)
                    rows.Add(new SettingRow { Key = kvp.Key, Value = kvp.Value });
                return rows;
            }
        }
    }
}
