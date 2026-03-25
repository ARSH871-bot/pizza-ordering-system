namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Applies promotional discount codes to an order total.
    /// </summary>
    public interface IPromoEngine
    {
        /// <summary>
        /// Attempts to apply <paramref name="code"/> to <paramref name="originalTotal"/>.
        /// Returns a <see cref="PromoResult"/> indicating success, the discounted total, and a user-facing message.
        /// </summary>
        PromoResult Apply(string code, decimal originalTotal);
    }
}
