using System;
using WindowsFormsApplication3.Config;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Applies promotional discount codes to an order total.
    /// All valid codes live in AppConfig — never hard-coded here.
    /// </summary>
    public class PromoEngine : IPromoEngine
    {
        public PromoResult Apply(string code, decimal originalTotal)
        {
            if (string.IsNullOrWhiteSpace(code))
                return new PromoResult { Success = false, Message = "Please enter a promo code." };

            string normalised = code.Trim().ToUpper();

            decimal discountRate;
            if (!AppConfig.PromoCodes.TryGetValue(normalised, out discountRate))
                return new PromoResult { Success = false, Message = "Invalid promo code. Please try again." };

            decimal discountedTotal = Math.Round(originalTotal * (1m - discountRate), 2);
            int     pct             = (int)(discountRate * 100);
            string  msg             = pct == 100
                ? "100% off — Free Order!"
                : $"{pct}% off applied. New total: {discountedTotal:C2}";

            return new PromoResult { Success = true, DiscountedTotal = discountedTotal, Message = msg };
        }
    }
}
