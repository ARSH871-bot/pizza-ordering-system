using System;

namespace WindowsFormsApplication3.Models
{
    /// <summary>Aggregated totals for a single calendar day.</summary>
    public class DailySummary
    {
        public DateTime Day         { get; set; }
        public int      OrderCount  { get; set; }
        public decimal  Revenue     { get; set; }
        public decimal  Gst         { get; set; }
    }

    /// <summary>Revenue and quantity for a single menu item over a period.</summary>
    public class TopItem
    {
        public string  Item         { get; set; }
        public int     TotalQty     { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    /// <summary>Order count and revenue broken down by payment method.</summary>
    public class PaymentSplit
    {
        public string  PaymentMethod { get; set; }
        public int     OrderCount    { get; set; }
        public decimal Revenue       { get; set; }
    }
}
