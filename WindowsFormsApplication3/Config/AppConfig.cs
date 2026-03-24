using System.Collections.Generic;
using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Config
{
    /// <summary>
    /// Single source of truth for all application constants.
    /// No magic numbers or strings anywhere else in the codebase.
    /// </summary>
    public static class AppConfig
    {
        // ── Tax ─────────────────────────────────────────────────────────────
        public const decimal TaxRate    = 0.15m;   // NZ GST
        public const string  TaxLabel   = "GST";
        public const string  CurrencyCode = "NZD";

        // ── Pizza prices ─────────────────────────────────────────────────────
        public static readonly IReadOnlyDictionary<PizzaSize, decimal> PizzaPrices =
            new Dictionary<PizzaSize, decimal>
            {
                { PizzaSize.Small,       4.00m },
                { PizzaSize.Medium,      7.00m },
                { PizzaSize.Large,      10.00m },
                { PizzaSize.ExtraLarge, 13.00m },
            };

        // ── Add-on prices ────────────────────────────────────────────────────
        public const decimal ToppingPrice  = 0.75m;
        public const decimal DrinkCanPrice = 1.45m;
        public const decimal WaterPrice    = 1.25m;
        public const decimal SidePrice     = 3.00m;

        // ── Promo codes (code → discount rate 0–1) ───────────────────────────
        public static readonly IReadOnlyDictionary<string, decimal> PromoCodes =
            new Dictionary<string, decimal>
            {
                { "PIZZA10",  0.10m },
                { "PIZZA20",  0.20m },
                { "FREESHIP", 1.00m },
            };

        // ── Delivery estimate ────────────────────────────────────────────────
        public const int DeliveryMinutes = 30;

        // ── NZ regions ───────────────────────────────────────────────────────
        public static readonly IReadOnlyList<string> NZRegions = new List<string>
        {
            "Auckland", "Bay of Plenty", "Canterbury", "Gisborne",
            "Hawke's Bay", "Manawatū-Whanganui", "Marlborough", "Nelson",
            "Northland", "Otago", "Southland", "Taranaki",
            "Tasman", "Waikato", "Wellington", "West Coast",
        };

        // ── Payment methods ──────────────────────────────────────────────────
        public static readonly IReadOnlyList<string> PaymentMethods = new List<string>
        {
            "Cash", "Credit Card", "Debit Card", "Promo Card",
        };
    }
}
