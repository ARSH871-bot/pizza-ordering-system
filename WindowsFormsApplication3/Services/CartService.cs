using System;
using System.Collections.Generic;
using WindowsFormsApplication3.Config;
using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Builds order line items from raw user selections.
    /// Contains no WinForms dependency — fully unit-testable.
    /// </summary>
    public class CartService : ICartService
    {
        /// <inheritdoc/>
        public List<OrderItem> BuildPizzaItems(PizzaSize size, CrustType crust, int qty,
                                               IList<string> selectedToppings)
        {
            if (qty < 1) throw new ArgumentOutOfRangeException("qty", "Quantity must be at least 1.");

            var items = new List<OrderItem>();

            decimal unitPrice  = AppConfig.PizzaPrices[size];
            decimal totalPrice = unitPrice * qty;
            string  sizeName   = size == PizzaSize.ExtraLarge ? "Extra Large" : size.ToString();
            string  crustName  = crust.ToString();

            items.Add(new OrderItem($"{crustName} Crust {sizeName} Pizza", qty, unitPrice));

            if (selectedToppings != null)
            {
                foreach (string topping in selectedToppings)
                {
                    if (!string.IsNullOrWhiteSpace(topping))
                        items.Add(new OrderItem($"  {topping} Toppings", 0, AppConfig.ToppingPrice));
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
    }
}
