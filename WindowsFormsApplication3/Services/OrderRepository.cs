using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Persists and retrieves <see cref="OrderRecord"/> instances using
    /// Newline-Delimited JSON (NDJSON) in <c>%APPDATA%\PizzaExpress\orders.ndjson</c>.
    /// Each <see cref="Save"/> appends exactly one line — O(1) write regardless
    /// of history size. <see cref="LoadAll"/> reads line-by-line.
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly string _dataDir;
        private readonly string _filePath;
        private readonly JavaScriptSerializer _serialiser = new JavaScriptSerializer();

        /// <summary>
        /// Initialises the repository using the default data directory:
        /// <c>%APPDATA%\PizzaExpress\orders.ndjson</c>.
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
            _filePath = Path.Combine(_dataDir, "orders.ndjson");
        }

        /// <inheritdoc/>
        /// <remarks>Appends one JSON line — constant-time write.</remarks>
        public void Save(OrderRecord record)
        {
            if (record == null) throw new ArgumentNullException("record");

            Directory.CreateDirectory(_dataDir);

            // Append a single JSON line (NDJSON format)
            string line = _serialiser.Serialize(record);
            File.AppendAllText(_filePath, line + Environment.NewLine);
        }

        /// <inheritdoc/>
        /// <remarks>Reads line-by-line; silently skips corrupted lines.</remarks>
        public List<OrderRecord> LoadAll()
        {
            if (!File.Exists(_filePath))
                return TryMigrateLegacy();

            var records = new List<OrderRecord>();
            foreach (string line in File.ReadLines(_filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var record = _serialiser.Deserialize<OrderRecord>(line);
                    if (record != null) records.Add(record);
                }
                catch
                {
                    // Corrupted line — skip it, never crash
                }
            }
            return records;
        }

        /// <summary>
        /// One-time migration: if the old <c>orders.json</c> (full-array format) exists
        /// but the new NDJSON file does not, import the old records and delete the old file.
        /// </summary>
        private List<OrderRecord> TryMigrateLegacy()
        {
            string legacyPath = Path.Combine(_dataDir, "orders.json");
            if (!File.Exists(legacyPath)) return new List<OrderRecord>();

            try
            {
                string json    = File.ReadAllText(legacyPath);
                var    records = _serialiser.Deserialize<List<OrderRecord>>(json)
                                 ?? new List<OrderRecord>();

                // Write each record into the new NDJSON file
                Directory.CreateDirectory(_dataDir);
                foreach (var r in records)
                    File.AppendAllText(_filePath, _serialiser.Serialize(r) + Environment.NewLine);

                // Rename the old file so migration only runs once
                File.Move(legacyPath, legacyPath + ".migrated");
                return records;
            }
            catch
            {
                return new List<OrderRecord>();
            }
        }
    }
}
