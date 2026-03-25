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
    }
}
