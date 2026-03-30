namespace WindowsFormsApplication3.Models
{
    /// <summary>
    /// Aggregate statistics returned by <see cref="Services.IOrderRepository.GetSummary"/>.
    /// </summary>
    public class OrderSummary
    {
        /// <summary>Total number of orders in the store.</summary>
        public int TotalOrders { get; set; }

        /// <summary>Sum of all order totals (inclusive of GST), rounded to 2 d.p.</summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>Average order total, rounded to 2 d.p. Zero when no orders exist.</summary>
        public decimal AverageOrderValue { get; set; }
    }
}
