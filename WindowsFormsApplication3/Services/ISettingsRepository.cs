using System.Collections.Generic;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Persists application settings (prices, delivery time, etc.) as key-value pairs
    /// in the SQLite <c>Settings</c> table created by migration <c>0001_AddSettingsTable</c>.
    /// </summary>
    public interface ISettingsRepository
    {
        /// <summary>
        /// Returns the stored value for <paramref name="key"/>, or
        /// <paramref name="defaultValue"/> if the key does not exist.
        /// </summary>
        string Get(string key, string defaultValue = null);

        /// <summary>Inserts or replaces the value for <paramref name="key"/>.</summary>
        void Set(string key, string value);

        /// <summary>
        /// Returns every setting row ordered by key, for display in the Settings UI.
        /// </summary>
        IReadOnlyList<SettingRow> GetAll();
    }

    /// <summary>A single row in the <c>Settings</c> table.</summary>
    public sealed class SettingRow
    {
        public string Key   { get; set; }
        public string Value { get; set; }
    }
}
