using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Persists and retrieves OrderRecords as JSON in %APPDATA%\PizzaExpress\orders.json.
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly string _dataDir;
        private readonly string _filePath;
        private readonly JavaScriptSerializer _serialiser = new JavaScriptSerializer();

        /// <summary>
        /// Initialises the repository using the default data directory:
        /// <c>%APPDATA%\PizzaExpress\orders.json</c>.
        /// </summary>
        public OrderRepository()
            : this(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PizzaExpress"))
        {
        }

        /// <summary>
        /// Initialises the repository using a custom <paramref name="dataDirectory"/>.
        /// Primarily used by unit tests to avoid touching real user data.
        /// </summary>
        public OrderRepository(string dataDirectory)
        {
            _dataDir  = dataDirectory;
            _filePath = Path.Combine(_dataDir, "orders.json");
        }

        /// <inheritdoc/>
        public void Save(OrderRecord record)
        {
            if (record == null) throw new ArgumentNullException("record");

            List<OrderRecord> all = LoadAll();
            all.Add(record);

            Directory.CreateDirectory(_dataDir);
            File.WriteAllText(_filePath, _serialiser.Serialize(all));
        }

        /// <inheritdoc/>
        public List<OrderRecord> LoadAll()
        {
            if (!File.Exists(_filePath))
                return new List<OrderRecord>();

            try
            {
                string json = File.ReadAllText(_filePath);
                return _serialiser.Deserialize<List<OrderRecord>>(json) ?? new List<OrderRecord>();
            }
            catch
            {
                // Corrupted file — return empty list rather than crash
                return new List<OrderRecord>();
            }
        }
    }
}
