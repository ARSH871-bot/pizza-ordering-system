using System;
using System.Collections.Generic;

namespace WindowsFormsApplication3.Models
{
    /// <summary>
    /// Serialisable snapshot of a completed order, persisted to JSON.
    /// </summary>
    public class OrderRecord
    {
        public string   Id            { get; set; }
        public DateTime OrderDate     { get; set; }
        public string   CustomerName  { get; set; }
        public string   Address       { get; set; }
        public string   City          { get; set; }
        public string   Region        { get; set; }
        public string   PostalCode    { get; set; }
        public string   PaymentMethod { get; set; }
        public decimal  Subtotal      { get; set; }
        public decimal  Tax           { get; set; }
        public decimal  Total         { get; set; }
        public List<OrderLineRecord> Lines { get; set; } = new List<OrderLineRecord>();
    }

    public class OrderLineRecord
    {
        public string  Item     { get; set; }
        public int     Quantity { get; set; }
        public decimal Price    { get; set; }
    }
}
