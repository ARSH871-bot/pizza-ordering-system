using System;
using System.Collections.Generic;
using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Persists and retrieves completed order records.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>Appends <paramref name="record"/> to the persistent order store.</summary>
        void Save(OrderRecord record);

        /// <summary>Returns all previously saved order records, or an empty list if none exist.</summary>
        List<OrderRecord> LoadAll();

        /// <summary>
        /// Returns orders matching <paramref name="text"/> (customer name, region, or payment method)
        /// and/or a date range.  Pass <c>null</c> / empty text and <c>null</c> dates to return all orders.
        /// Results are ordered oldest-first.
        /// </summary>
        List<OrderRecord> Search(string text, DateTime? from, DateTime? to);

        /// <summary>Returns aggregate statistics across all saved orders.</summary>
        OrderSummary GetSummary();

        /// <summary>
        /// Permanently removes the order with the given <paramref name="id"/> from the store,
        /// including all associated line items.  No-ops silently if the id does not exist.
        /// </summary>
        void Delete(string id);

        /// <summary>Marks an order as "Voided" without removing it from the database.</summary>
        void VoidOrder(string id);

        /// <summary>
        /// Returns aggregate statistics for orders in the given date range.
        /// Pass <c>null</c> for both bounds to aggregate all-time.
        /// Only "Active" orders are counted.
        /// </summary>
        OrderSummary GetSummaryForPeriod(DateTime? from, DateTime? to);

        /// <summary>
        /// Returns one row per calendar day in the given range, ordered by date.
        /// Only "Active" orders are counted.
        /// </summary>
        List<DailySummary> GetDailySummaries(DateTime from, DateTime to);

        /// <summary>
        /// Returns the top <paramref name="limit"/> menu items by revenue in the given period.
        /// Topping rows (indented names) are excluded.
        /// Only "Active" orders are counted.
        /// </summary>
        List<TopItem> GetTopItems(DateTime? from, DateTime? to, int limit);

        /// <summary>
        /// Returns order count and revenue grouped by payment method for the given period.
        /// Only "Active" orders are counted.
        /// </summary>
        List<PaymentSplit> GetPaymentBreakdown(DateTime? from, DateTime? to);
    }
}
