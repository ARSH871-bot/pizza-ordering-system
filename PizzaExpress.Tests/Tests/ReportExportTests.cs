using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3;
using WindowsFormsApplication3.Forms;
using WindowsFormsApplication3.Models;

namespace PizzaExpress.Tests.Tests
{
    [TestClass]
    public class ReportExportTests
    {
        private static readonly CultureInfo NZD = new CultureInfo("en-NZ");

        // ── Helpers ───────────────────────────────────────────────────────────

        private static OrderSummary MakeSummary(int orders = 5, decimal revenue = 200m)
            => new OrderSummary
            {
                TotalOrders      = orders,
                TotalRevenue     = revenue,
                AverageOrderValue = orders > 0 ? revenue / orders : 0m,
            };

        private static List<DailySummary> MakeDailySummaries()
            => new List<DailySummary>
            {
                new DailySummary { Day = new DateTime(2026, 5, 1), OrderCount = 3, Revenue = 90m, Gst = 90m - 90m / 1.15m },
                new DailySummary { Day = new DateTime(2026, 5, 2), OrderCount = 2, Revenue = 110m, Gst = 110m - 110m / 1.15m },
            };

        private static List<TopItem> MakeTopItems()
            => new List<TopItem>
            {
                new TopItem { Item = "Large Pizza", TotalQty = 4, TotalRevenue = 80m },
                new TopItem { Item = "Coke", TotalQty = 6, TotalRevenue = 18m },
            };

        private static List<PaymentSplit> MakePayments()
            => new List<PaymentSplit>
            {
                new PaymentSplit { PaymentMethod = "Cash",        OrderCount = 3, Revenue = 120m },
                new PaymentSplit { PaymentMethod = "Credit Card", OrderCount = 2, Revenue = 80m  },
            };

        // ── OrderHistoryForm.BuildHistoryCsv ──────────────────────────────────

        [TestMethod]
        public void BuildHistoryCsv_EmptyList_ReturnsHeaderOnly()
        {
            string csv = OrderHistoryForm.BuildHistoryCsv(new List<OrderRecord>());
            StringAssert.Contains(csv, "Date/Time,Customer,Region,Payment,Total NZD");
            Assert.AreEqual(1, csv.Split('\n').Count(l => l.Trim().Length > 0));
        }

        [TestMethod]
        public void BuildHistoryCsv_SingleRecord_ContainsAllFields()
        {
            var record = new OrderRecord
            {
                OrderDate     = new DateTime(2026, 5, 11, 14, 30, 0),
                CustomerName  = "Jamie Taylor",
                Region        = "Auckland",
                PaymentMethod = "Cash",
                Total         = 42.50m,
            };

            string csv = OrderHistoryForm.BuildHistoryCsv(new[] { record });

            StringAssert.Contains(csv, "\"2026-05-11 14:30:00\"");
            StringAssert.Contains(csv, "\"Jamie Taylor\"");
            StringAssert.Contains(csv, "\"Auckland\"");
            StringAssert.Contains(csv, "\"Cash\"");
            StringAssert.Contains(csv, "42.50");
        }

        [TestMethod]
        public void BuildHistoryCsv_MultipleRecords_OneDataRowEach()
        {
            var records = new List<OrderRecord>
            {
                new OrderRecord { OrderDate = new DateTime(2026, 5, 1), CustomerName = "A", Region = "Wellington", PaymentMethod = "Cash", Total = 10m },
                new OrderRecord { OrderDate = new DateTime(2026, 5, 2), CustomerName = "B", Region = "Christchurch", PaymentMethod = "Debit Card", Total = 20m },
            };

            string csv = OrderHistoryForm.BuildHistoryCsv(records);
            string[] lines = csv.Split('\n').Where(l => l.Trim().Length > 0).ToArray();

            Assert.AreEqual(3, lines.Length, "Header + 2 data rows expected.");
            StringAssert.Contains(lines[1], "\"A\"");
            StringAssert.Contains(lines[2], "\"B\"");
        }

        [TestMethod]
        public void BuildHistoryCsv_SpecialCharactersInName_AreQuoted()
        {
            var record = new OrderRecord
            {
                OrderDate     = new DateTime(2026, 5, 1),
                CustomerName  = "O'Brien, Sean",
                Region        = "Auckland",
                PaymentMethod = "Cash",
                Total         = 15m,
            };

            string csv = OrderHistoryForm.BuildHistoryCsv(new[] { record });
            StringAssert.Contains(csv, "\"O'Brien, Sean\"");
        }

        // ── SalesReportForm.BuildSalesReportCsv ───────────────────────────────

        [TestMethod]
        public void BuildSalesReportCsv_ContainsHeader()
        {
            string csv = SalesReportForm.BuildSalesReportCsv(
                new DateTime(2026, 5, 1), new DateTime(2026, 5, 31),
                MakeSummary(), MakeDailySummaries(), MakeTopItems(), MakePayments());

            StringAssert.Contains(csv, "=== PIZZA EXPRESS — SALES REPORT ===");
        }

        [TestMethod]
        public void BuildSalesReportCsv_ContainsPeriodLine()
        {
            string csv = SalesReportForm.BuildSalesReportCsv(
                new DateTime(2026, 5, 1), new DateTime(2026, 5, 31),
                MakeSummary(), MakeDailySummaries(), MakeTopItems(), MakePayments());

            StringAssert.Contains(csv, "Period: 2026-05-01 to 2026-05-31");
        }

        [TestMethod]
        public void BuildSalesReportCsv_ContainsDailyRows()
        {
            string csv = SalesReportForm.BuildSalesReportCsv(
                new DateTime(2026, 5, 1), new DateTime(2026, 5, 31),
                MakeSummary(), MakeDailySummaries(), MakeTopItems(), MakePayments());

            StringAssert.Contains(csv, "DATE,ORDERS,REVENUE,GST");
            StringAssert.Contains(csv, "2026-05-01,3,");
            StringAssert.Contains(csv, "2026-05-02,2,");
        }

        [TestMethod]
        public void BuildSalesReportCsv_ContainsItemRows()
        {
            string csv = SalesReportForm.BuildSalesReportCsv(
                new DateTime(2026, 5, 1), new DateTime(2026, 5, 31),
                MakeSummary(), MakeDailySummaries(), MakeTopItems(), MakePayments());

            StringAssert.Contains(csv, "ITEM,QTY,REVENUE");
            StringAssert.Contains(csv, "\"Large Pizza\",4,");
            StringAssert.Contains(csv, "\"Coke\",6,");
        }

        [TestMethod]
        public void BuildSalesReportCsv_ContainsPaymentRows()
        {
            string csv = SalesReportForm.BuildSalesReportCsv(
                new DateTime(2026, 5, 1), new DateTime(2026, 5, 31),
                MakeSummary(), MakeDailySummaries(), MakeTopItems(), MakePayments());

            StringAssert.Contains(csv, "PAYMENT METHOD,ORDERS,REVENUE");
            StringAssert.Contains(csv, "Cash,3,");
            StringAssert.Contains(csv, "Credit Card,2,");
        }

        [TestMethod]
        public void BuildSalesReportCsv_EmptyData_OnlyContainsSectionHeaders()
        {
            string csv = SalesReportForm.BuildSalesReportCsv(
                new DateTime(2026, 5, 1), new DateTime(2026, 5, 1),
                MakeSummary(0, 0m),
                new List<DailySummary>(),
                new List<TopItem>(),
                new List<PaymentSplit>());

            StringAssert.Contains(csv, "DATE,ORDERS,REVENUE,GST");
            StringAssert.Contains(csv, "ITEM,QTY,REVENUE");
            StringAssert.Contains(csv, "PAYMENT METHOD,ORDERS,REVENUE");
        }

        [TestMethod]
        public void BuildSalesReportCsv_NullPaymentMethod_UsesUnknown()
        {
            var payments = new List<PaymentSplit>
            {
                new PaymentSplit { PaymentMethod = null, OrderCount = 1, Revenue = 30m },
            };

            string csv = SalesReportForm.BuildSalesReportCsv(
                new DateTime(2026, 5, 1), new DateTime(2026, 5, 1),
                MakeSummary(1, 30m),
                new List<DailySummary>(),
                new List<TopItem>(),
                payments);

            StringAssert.Contains(csv, "Unknown,1,");
        }

        // ── EndOfDayForm.BuildZReportCsv ──────────────────────────────────────

        [TestMethod]
        public void BuildZReportCsv_ContainsHeader()
        {
            string csv = EndOfDayForm.BuildZReportCsv(
                new DateTime(2026, 5, 11), MakeSummary(), MakePayments(), MakeTopItems());

            StringAssert.Contains(csv, "=== PIZZA EXPRESS — Z-REPORT ===");
        }

        [TestMethod]
        public void BuildZReportCsv_ContainsDateLine()
        {
            string csv = EndOfDayForm.BuildZReportCsv(
                new DateTime(2026, 5, 11), MakeSummary(), MakePayments(), MakeTopItems());

            StringAssert.Contains(csv, "Date: 2026-05-11");
        }

        [TestMethod]
        public void BuildZReportCsv_ContainsPaymentRows()
        {
            string csv = EndOfDayForm.BuildZReportCsv(
                new DateTime(2026, 5, 11), MakeSummary(), MakePayments(), MakeTopItems());

            StringAssert.Contains(csv, "PAYMENT METHOD,ORDERS,REVENUE");
            StringAssert.Contains(csv, "Cash,3,");
            StringAssert.Contains(csv, "Credit Card,2,");
        }

        [TestMethod]
        public void BuildZReportCsv_ContainsItemRows()
        {
            string csv = EndOfDayForm.BuildZReportCsv(
                new DateTime(2026, 5, 11), MakeSummary(), MakePayments(), MakeTopItems());

            StringAssert.Contains(csv, "ITEM,QTY,REVENUE");
            StringAssert.Contains(csv, "\"Large Pizza\",4,");
        }

        [TestMethod]
        public void BuildZReportCsv_ItemWithZeroQty_FallsBackToOne()
        {
            var items = new List<TopItem>
            {
                new TopItem { Item = "Side Salad", TotalQty = 0, TotalRevenue = 5m },
            };

            string csv = EndOfDayForm.BuildZReportCsv(
                new DateTime(2026, 5, 11), MakeSummary(), MakePayments(), items);

            StringAssert.Contains(csv, "\"Side Salad\",1,");
        }

        // ── EndOfDayForm.BuildZReportText ─────────────────────────────────────

        [TestMethod]
        public void BuildZReportText_ContainsSeparatorAndTitle()
        {
            string text = EndOfDayForm.BuildZReportText(
                new DateTime(2026, 5, 11), MakeSummary(), MakePayments(), MakeTopItems(),
                printedAt: new DateTime(2026, 5, 11, 23, 0, 0));

            StringAssert.Contains(text, "PIZZA EXPRESS NZ — Z-REPORT");
            StringAssert.Contains(text, new string('=', 44));
        }

        [TestMethod]
        public void BuildZReportText_ContainsDateLine()
        {
            string text = EndOfDayForm.BuildZReportText(
                new DateTime(2026, 5, 11), MakeSummary(), MakePayments(), MakeTopItems(),
                printedAt: new DateTime(2026, 5, 11, 23, 0, 0));

            StringAssert.Contains(text, "2026");
        }

        [TestMethod]
        public void BuildZReportText_ContainsPrintedTimestamp()
        {
            string text = EndOfDayForm.BuildZReportText(
                new DateTime(2026, 5, 11), MakeSummary(), MakePayments(), MakeTopItems(),
                printedAt: new DateTime(2026, 5, 11, 23, 59, 0));

            StringAssert.Contains(text, "Printed");
            StringAssert.Contains(text, "2026-05-11");
        }

        [TestMethod]
        public void BuildZReportText_ContainsPaymentBreakdownSection()
        {
            string text = EndOfDayForm.BuildZReportText(
                new DateTime(2026, 5, 11), MakeSummary(), MakePayments(), MakeTopItems(),
                printedAt: new DateTime(2026, 5, 11, 23, 0, 0));

            StringAssert.Contains(text, "PAYMENT BREAKDOWN");
            StringAssert.Contains(text, "Cash");
            StringAssert.Contains(text, "Credit Card");
        }

        [TestMethod]
        public void BuildZReportText_ContainsTopItemsSection()
        {
            string text = EndOfDayForm.BuildZReportText(
                new DateTime(2026, 5, 11), MakeSummary(), MakePayments(), MakeTopItems(),
                printedAt: new DateTime(2026, 5, 11, 23, 0, 0));

            StringAssert.Contains(text, "TOP ITEMS");
            StringAssert.Contains(text, "Large Pizza");
            StringAssert.Contains(text, "Coke");
        }

        [TestMethod]
        public void BuildZReportText_EmptyPayments_ShowsNoSalesMessage()
        {
            string text = EndOfDayForm.BuildZReportText(
                new DateTime(2026, 5, 11),
                MakeSummary(0, 0m),
                new List<PaymentSplit>(),
                new List<TopItem>(),
                printedAt: new DateTime(2026, 5, 11, 23, 0, 0));

            StringAssert.Contains(text, "(no sales)");
            StringAssert.Contains(text, "(no items sold)");
        }
    }
}
