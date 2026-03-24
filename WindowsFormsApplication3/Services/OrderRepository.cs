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
    public class OrderRepository
    {
        private static readonly string DataDir  = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PizzaExpress");
        private static readonly string FilePath = Path.Combine(DataDir, "orders.json");

        private readonly JavaScriptSerializer _serialiser = new JavaScriptSerializer();

        public void Save(OrderRecord record)
        {
            if (record == null) throw new ArgumentNullException("record");

            List<OrderRecord> all = LoadAll();
            all.Add(record);

            Directory.CreateDirectory(DataDir);
            File.WriteAllText(FilePath, _serialiser.Serialize(all));
        }

        public List<OrderRecord> LoadAll()
        {
            if (!File.Exists(FilePath))
                return new List<OrderRecord>();

            try
            {
                string json = File.ReadAllText(FilePath);
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
