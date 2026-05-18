using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Forms;
using WindowsFormsApplication3.Infrastructure;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    [DoNotParallelize]
    [TestClass]
    public class PinHardeningTests
    {
        [TestMethod]
        public void SettingsForm_SaveChanges_WithNewStaffPin_StoresProtectedHash()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var settings = new SettingsRepository(tempDir);

                    using (var form = new SettingsForm(settings, tempDir))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        DataGridView grid = WinFormsTestHelper.EnumerateControls<DataGridView>(form).Single();
                        DataGridViewRow pinRow = grid.Rows
                            .Cast<DataGridViewRow>()
                            .Single(row => string.Equals(row.Tag as string, "StaffPin", StringComparison.Ordinal));

                        pinRow.Cells["Value"].Value = "1234";

                        using (new WinFormsTestHelper.DialogAutoCloser("Saved"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "Save Changes").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                    }

                    string storedPin = settings.Get("StaffPin");
                    Assert.AreNotEqual("1234", storedPin);
                    Assert.IsTrue(PinSecurity.IsProtected(storedPin));
                    Assert.IsTrue(PinSecurity.Verify("1234", storedPin));
                }
                finally
                {
                    DeleteTempDataDirectory(tempDir);
                }
            });
        }

        [TestMethod]
        public void SettingsForm_Load_WithConfiguredPin_DoesNotExposeStoredValue()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    string protectedPin = PinSecurity.Protect("1234");
                    settings.Set("StaffPin", protectedPin);

                    using (var form = new SettingsForm(settings, tempDir))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        DataGridView grid = WinFormsTestHelper.EnumerateControls<DataGridView>(form).Single();
                        DataGridViewRow pinRow = grid.Rows
                            .Cast<DataGridViewRow>()
                            .Single(row => string.Equals(row.Tag as string, "StaffPin", StringComparison.Ordinal));

                        string displayed = Convert.ToString(pinRow.Cells["Value"].Value);
                        Assert.AreNotEqual(protectedPin, displayed);
                        Assert.IsFalse(string.IsNullOrWhiteSpace(displayed));
                        StringAssert.Contains(displayed, "configured");
                    }
                }
                finally
                {
                    DeleteTempDataDirectory(tempDir);
                }
            });
        }

        [TestMethod]
        public void PinLoginForm_WithProtectedPin_AllowsUnlock()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    ResetStaffAuthSession();
                    DatabaseMigrator.Run(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    settings.Set("StaffPin", PinSecurity.Protect("1234"));

                    using (var form = new PinLoginForm(settings))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        EnterDigits(form, "1234");
                        WinFormsTestHelper.FindByTextPrefix<Button>(form, "Unlock").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        Assert.AreEqual(DialogResult.OK, form.DialogResult);
                        Assert.IsTrue(StaffAuthSession.HasRecentAuthorization(TimeSpan.FromMinutes(1)));
                    }
                }
                finally
                {
                    ResetStaffAuthSession();
                    DeleteTempDataDirectory(tempDir);
                }
            });
        }

        [TestMethod]
        public void PinLoginForm_WithLegacyPlainTextPin_UpgradesToProtectedHashAfterSuccessfulUnlock()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    settings.Set("StaffPin", "1234");

                    using (var form = new PinLoginForm(settings))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        EnterDigits(form, "1234");
                        WinFormsTestHelper.FindByTextPrefix<Button>(form, "Unlock").PerformClick();
                        WinFormsTestHelper.PumpEvents();
                    }

                    string storedPin = settings.Get("StaffPin");
                    Assert.IsTrue(PinSecurity.IsProtected(storedPin));
                    Assert.IsTrue(PinSecurity.Verify("1234", storedPin));
                }
                finally
                {
                    DeleteTempDataDirectory(tempDir);
                }
            });
        }

        [TestMethod]
        public void PinLoginForm_ThreeIncorrectAttempts_DisablesUnlockTemporarily()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    settings.Set("StaffPin", PinSecurity.Protect("1234"));

                    using (var form = new PinLoginForm(settings))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        for (int i = 0; i < 3; i++)
                        {
                            EnterDigits(form, "9999");
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "Unlock").PerformClick();
                            WinFormsTestHelper.PumpEvents();
                        }

                        Button unlock = WinFormsTestHelper.GetPrivateField<Button>(form, "_btnEnter");
                        Label error = WinFormsTestHelper.GetPrivateField<Label>(form, "_lblError");

                        Assert.IsFalse(unlock.Enabled);
                        StringAssert.Contains(error.Text, "Try again");
                    }
                }
                finally
                {
                    DeleteTempDataDirectory(tempDir);
                }
            });
        }

        [TestMethod]
        public void PinLoginForm_EnsureAuthorized_WhenPinDisabled_ReturnsTrueWithoutPrompt()
        {
            string tempDir = CreateTempDataDirectory();
            try
            {
                DatabaseMigrator.Run(tempDir);
                var settings = new SettingsRepository(tempDir);
                settings.Set("StaffPin", string.Empty);

                Assert.IsTrue(PinLoginForm.EnsureAuthorized(null, settings, TimeSpan.FromMinutes(10)));
            }
            finally
            {
                DeleteTempDataDirectory(tempDir);
            }
        }

        [TestMethod]
        public void PinLoginForm_EnsureAuthorized_WhenSessionRecent_ReturnsTrueWithoutPrompt()
        {
            string tempDir = CreateTempDataDirectory();
            try
            {
                ResetStaffAuthSession();
                DatabaseMigrator.Run(tempDir);
                var settings = new SettingsRepository(tempDir);
                settings.Set("StaffPin", PinSecurity.Protect("1234"));
                StaffAuthSession.MarkAuthenticated();

                Assert.IsTrue(PinLoginForm.EnsureAuthorized(null, settings, TimeSpan.FromMinutes(10)));
            }
            finally
            {
                ResetStaffAuthSession();
                DeleteTempDataDirectory(tempDir);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PinSecurity_Protect_TooShortPin_ThrowsArgumentException()
        {
            PinSecurity.Protect("12"); // below minimum length — covers the throw on line 29
        }

        [TestMethod]
        public void PinSecurity_ConstantTimeEquals_DifferentLengths_ReturnsFalse()
        {
            var mi = typeof(PinSecurity).GetMethod(
                "ConstantTimeEquals",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(mi, "ConstantTimeEquals not found via reflection.");

            bool result = (bool)mi.Invoke(null, new object[]
            {
                new byte[] { 1, 2, 3 },
                new byte[] { 1, 2 },
            });
            Assert.IsFalse(result, "Different-length byte arrays must not be considered equal.");
        }

        private static void EnterDigits(Control form, string digits)
        {
            foreach (char digit in digits)
            {
                WinFormsTestHelper.FindByTextPrefix<Button>(form, digit.ToString()).PerformClick();
                WinFormsTestHelper.PumpEvents();
            }
        }

        private static string CreateTempDataDirectory()
        {
            string dir = Path.Combine(Path.GetTempPath(), "PizzaExpressPin_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static void DeleteTempDataDirectory(string tempDir)
        {
            if (!string.IsNullOrWhiteSpace(tempDir) && Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        private static void ResetStaffAuthSession()
        {
            FieldInfo field = typeof(StaffAuthSession).GetField(
                "_lastAuthenticatedUtc",
                BindingFlags.Static | BindingFlags.NonPublic);

            Assert.IsNotNull(field);
            field.SetValue(null, DateTime.MinValue);
        }
    }
}
