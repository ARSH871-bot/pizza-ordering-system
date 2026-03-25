using System;

namespace WindowsFormsApplication3.Models
{
    /// <summary>
    /// A single line item in an order (pizza, drink, side, or topping).
    /// <see cref="TotalPrice"/> is computed as <c>UnitPrice × max(Quantity, 1)</c>, rounded to 2 d.p.
    /// </summary>
    public class OrderItem
    {
        public string  Name      { get; set; }
        public int     Quantity  { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => Math.Round(UnitPrice * Math.Max(Quantity, 1), 2);

        public OrderItem() { }

        public OrderItem(string name, int quantity, decimal unitPrice)
        {
            Name      = name;
            Quantity  = quantity;
            UnitPrice = unitPrice;
        }
    }
}
