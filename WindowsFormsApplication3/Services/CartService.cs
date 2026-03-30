using System;
using System.Collections.Generic;
using WindowsFormsApplication3.Config;
using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Builds order line items from raw user selections.
    /// Contains no WinForms dependency — fully unit-testable.
    /// <para>
    /// When constructed with an <see cref="ISettingsRepository"/>, prices are read from
    /// the database so a business owner can adjust them without recompiling.
    /// The parameterless constructor falls back to the compile-time <see cref="AppConfig"/>
    /// constants, keeping all existing tests green.
    /// </para>
    /// </summary>
    public class CartService : ICartService
    {
        private readonly ISettingsRepository _settings;

        /// <summary>Parameterless constructor — uses compile-time <see cref="AppConfig"/> prices.</summary>
        public CartService() { }

        /// <summary>Constructor with database-backed settings — prices are resolved at runtime.</summary>
        public CartService(ISettingsRepository settings)
        {
            _settings = settings;
        }

        /// <inheritdoc/>
        public List<OrderItem> BuildPizzaItems(PizzaSize size, CrustType crust, int qty,
                                               IList<string> selectedToppings)
        {
            if (qty < 1) throw new ArgumentOutOfRangeException("qty", "Quantity must be at least 1.");

            var items = new List<OrderItem>();

            decimal unitPrice = GetPizzaPrice(size);
            string  sizeName  = size == PizzaSize.ExtraLarge ? "Extra Large" : size.ToString();
            string  crustName = crust.ToString();

            items.Add(new OrderItem($"{crustName} Crust {sizeName} Pizza", qty, unitPrice));

            if (selectedToppings != null)
            {
                foreach (string topping in selectedToppings)
                {
                    if (!string.IsNullOrWhiteSpace(topping))
                        items.Add(new OrderItem($"  {topping} Toppings", 0, GetToppingPrice()));
                }
            }

            return items;
        }

        /// <inheritdoc/>
        public decimal CalculateSubtotal(IEnumerable<OrderItem> items)
        {
            if (items == null) return 0m;
            decimal total = 0m;
            foreach (var item in items)
                total += item.TotalPrice;
            return total;
        }

        /// <inheritdoc/>
        public decimal CalculateTax(decimal subtotal)
            => Math.Round(subtotal * AppConfig.TaxRate, 2);

        /// <inheritdoc/>
        public decimal CalculateTotal(decimal subtotal)
            => subtotal + CalculateTax(subtotal);

        /// <inheritdoc/>
        public decimal GetDrinkCanPrice()
            => GetDecimalSetting("DrinkCanPrice", AppConfig.DrinkCanPrice);

        /// <inheritdoc/>
        public decimal GetWaterPrice()
            => GetDecimalSetting("WaterPrice", AppConfig.WaterPrice);

        /// <inheritdoc/>
        public decimal GetSidePrice()
            => GetDecimalSetting("SidePrice", AppConfig.SidePrice);

        // ── Price resolution: DB setting → AppConfig fallback ────────────────

        private decimal GetPizzaPrice(PizzaSize size)
        {
            return GetDecimalSetting($"PizzaPrice.{size}", AppConfig.PizzaPrices[size]);
        }

        private decimal GetToppingPrice()
        {
            return GetDecimalSetting("ToppingPrice", AppConfig.ToppingPrice);
        }

        private decimal GetDecimalSetting(string key, decimal fallback)
        {
            if (_settings == null) return fallback;

            string raw = _settings.Get(key);
            decimal parsed;
            if (raw != null && decimal.TryParse(raw, out parsed)) return parsed;
            return fallback;
        }
    }
}
