using System.Collections.Generic;
using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Builds order line items from raw user selections.
    /// Contains no WinForms dependency — fully unit-testable.
    /// </summary>
    public interface ICartService
    {
        /// <summary>
        /// Builds the pizza line item and any selected topping line items
        /// for one pizza configuration.
        /// </summary>
        /// <param name="size">The selected pizza size.</param>
        /// <param name="crust">The selected crust type.</param>
        /// <param name="qty">Number of pizzas (1–20).</param>
        /// <param name="selectedToppings">Display names of ticked toppings.</param>
        List<OrderItem> BuildPizzaItems(PizzaSize size, CrustType crust, int qty,
                                        IList<string> selectedToppings);

        /// <summary>
        /// Calculates the subtotal (excl. GST) for a collection of order items.
        /// </summary>
        decimal CalculateSubtotal(IEnumerable<OrderItem> items);

        /// <summary>
        /// Calculates the GST amount for the given subtotal using <see cref="Config.AppConfig.TaxRate"/>.
        /// </summary>
        decimal CalculateTax(decimal subtotal);

        /// <summary>
        /// Calculates the total (incl. GST) for the given subtotal.
        /// </summary>
        decimal CalculateTotal(decimal subtotal);

        /// <summary>
        /// Returns the current configured canned-drink price.
        /// </summary>
        decimal GetDrinkCanPrice();

        /// <summary>
        /// Returns the current configured bottled-water price.
        /// </summary>
        decimal GetWaterPrice();

        /// <summary>
        /// Returns the current configured side-item price.
        /// </summary>
        decimal GetSidePrice();
    }
}
