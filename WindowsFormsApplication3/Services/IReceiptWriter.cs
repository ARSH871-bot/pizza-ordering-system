using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Builds and persists order receipts.
    /// </summary>
    public interface IReceiptWriter
    {
        /// <summary>Builds a formatted receipt string from the given <paramref name="order"/>.</summary>
        string Build(Order order);

        /// <summary>Builds a receipt for <paramref name="order"/> and writes it to <paramref name="filePath"/>.</summary>
        void SaveToFile(Order order, string filePath);
    }
}
