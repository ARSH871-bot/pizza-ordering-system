using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Forms;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    [TestClass]
    public class SettingsFormLogicTests
    {
        // ── GetDisplayValue ───────────────────────────────────────────────────

        [TestMethod]
        public void GetDisplayValue_NullRow_ReturnsEmpty()
            => Assert.AreEqual(string.Empty, SettingsForm.GetDisplayValue(null));

        [TestMethod]
        public void GetDisplayValue_NonPinKey_ReturnsRowValueAsIs()
        {
            var row = new SettingRow { Key = "DrinkCanPrice", Value = "1.45" };
            Assert.AreEqual("1.45", SettingsForm.GetDisplayValue(row));
        }

        [TestMethod]
        public void GetDisplayValue_PinKey_NotConfigured_ReturnsEmpty()
        {
            var row = new SettingRow { Key = "StaffPin", Value = "" };
            Assert.AreEqual(string.Empty, SettingsForm.GetDisplayValue(row));
        }

        [TestMethod]
        public void GetDisplayValue_PinKey_Configured_ReturnsPlaceholder()
        {
            string hash = PinSecurity.Protect("1234");
            var row = new SettingRow { Key = "StaffPin", Value = hash };
            string result = SettingsForm.GetDisplayValue(row);
            Assert.AreEqual(SettingsForm.StaffPinConfiguredPlaceholder, result);
        }

        // ── TrySaveStaffPin ───────────────────────────────────────────────────

        private sealed class SpySettings : ISettingsRepository
        {
            private readonly Dictionary<string, string> _store = new Dictionary<string, string>();

            public string Get(string key, string defaultValue = null)
                => _store.TryGetValue(key, out var v) ? v : defaultValue;

            public void Set(string key, string value)
                => _store[key] = value;

            public IReadOnlyList<SettingRow> GetAll()
            {
                var list = new List<SettingRow>();
                foreach (var kv in _store)
                    list.Add(new SettingRow { Key = kv.Key, Value = kv.Value });
                return list;
            }
        }

        [TestMethod]
        public void TrySaveStaffPin_EmptyString_ClearsPinInSettings()
        {
            var settings = new SpySettings();
            settings.Set("StaffPin", PinSecurity.Protect("1234"));
            var errors = new StringBuilder();

            bool result = SettingsForm.TrySaveStaffPin(settings, "", errors);

            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, settings.Get("StaffPin", null));
            Assert.AreEqual(0, errors.Length);
        }

        [TestMethod]
        public void TrySaveStaffPin_NullValue_ClearsPinInSettings()
        {
            var settings = new SpySettings();
            settings.Set("StaffPin", "existing");
            var errors = new StringBuilder();

            bool result = SettingsForm.TrySaveStaffPin(settings, null, errors);

            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, settings.Get("StaffPin", null));
        }

        [TestMethod]
        public void TrySaveStaffPin_Placeholder_PinNotConfigured_ClearsPin()
        {
            var settings = new SpySettings();
            settings.Set("StaffPin", "");
            var errors = new StringBuilder();

            bool result = SettingsForm.TrySaveStaffPin(
                settings, SettingsForm.StaffPinConfiguredPlaceholder, errors);

            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, settings.Get("StaffPin", null));
        }

        [TestMethod]
        public void TrySaveStaffPin_Placeholder_PinConfiguredButPlaintext_UpgradesToProtected()
        {
            var settings = new SpySettings();
            settings.Set("StaffPin", "1234");
            var errors = new StringBuilder();

            bool result = SettingsForm.TrySaveStaffPin(
                settings, SettingsForm.StaffPinConfiguredPlaceholder, errors);

            Assert.IsTrue(result);
            string stored = settings.Get("StaffPin", null);
            Assert.IsTrue(PinSecurity.IsProtected(stored), "PIN should be upgraded to PBKDF2 hash");
            Assert.IsTrue(PinSecurity.Verify("1234", stored));
        }

        [TestMethod]
        public void TrySaveStaffPin_Placeholder_PinAlreadyProtected_NoChange()
        {
            var settings = new SpySettings();
            string original = PinSecurity.Protect("5678");
            settings.Set("StaffPin", original);
            var errors = new StringBuilder();

            bool result = SettingsForm.TrySaveStaffPin(
                settings, SettingsForm.StaffPinConfiguredPlaceholder, errors);

            Assert.IsTrue(result);
            Assert.AreEqual(original, settings.Get("StaffPin", null));
        }

        [TestMethod]
        public void TrySaveStaffPin_InvalidPin_TooShort_AddErrorAndReturnsFalse()
        {
            var settings = new SpySettings();
            settings.Set("StaffPin", "");
            var errors = new StringBuilder();

            bool result = SettingsForm.TrySaveStaffPin(settings, "123", errors);

            Assert.IsFalse(result);
            Assert.IsTrue(errors.Length > 0, "Errors should contain validation message");
        }

        [TestMethod]
        public void TrySaveStaffPin_ValidNewPin_StoresProtectedHash()
        {
            var settings = new SpySettings();
            settings.Set("StaffPin", "");
            var errors = new StringBuilder();

            bool result = SettingsForm.TrySaveStaffPin(settings, "9876", errors);

            Assert.IsTrue(result);
            string stored = settings.Get("StaffPin", null);
            Assert.IsTrue(PinSecurity.IsProtected(stored));
            Assert.IsTrue(PinSecurity.Verify("9876", stored));
            Assert.AreEqual(0, errors.Length);
        }
    }
}
