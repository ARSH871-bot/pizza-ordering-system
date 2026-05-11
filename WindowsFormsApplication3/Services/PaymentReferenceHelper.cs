using System.Text.RegularExpressions;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Normalises payment references before they are validated, displayed, or persisted.
    /// The app does not store raw card numbers; card-like input is masked to the last 4 digits.
    /// </summary>
    public static class PaymentReferenceHelper
    {
        private const int MaxReferenceLength = 30;

        public static bool RequiresReference(string paymentMethod)
        {
            return !string.IsNullOrWhiteSpace(paymentMethod)
                && paymentMethod != "Cash"
                && paymentMethod != "Promo Card";
        }

        public static string NormalizeForStorage(string paymentMethod, string rawReference)
        {
            if (!RequiresReference(paymentMethod))
                return null;

            string normalized = Regex.Replace((rawReference ?? string.Empty).Trim(), @"\s+", " ");
            if (string.IsNullOrWhiteSpace(normalized))
                return null;

            if (normalized.Length > MaxReferenceLength)
                normalized = normalized.Substring(0, MaxReferenceLength);

            string digitsOnly = Regex.Replace(normalized, @"\D", string.Empty);
            if (digitsOnly.Length >= 12)
                return "****" + digitsOnly.Substring(digitsOnly.Length - 4);

            return normalized;
        }
    }
}
