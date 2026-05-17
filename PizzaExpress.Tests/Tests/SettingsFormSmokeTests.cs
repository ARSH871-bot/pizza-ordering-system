using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Forms;
using WindowsFormsApplication3.Infrastructure;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    [DoNotParallelize]
    [TestClass]
    public class SettingsFormSmokeTests
    {
        // ── No data directory guard (ShowNoDataDir) ───────────────────────────

        [TestMethod]
        public void SettingsForm_BackupButton_WithNoDataDir_ShowsBackupUnavailableDialog()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var settings = new SettingsRepository(tempDir);

                    // Passing null for dataDirectory triggers ShowNoDataDir on backup
                    using (var form = new SettingsForm(settings, dataDirectory: null))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        using (new WinFormsTestHelper.DialogAutoCloser("Backup Unavailable"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "Backup DB").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                        Assert.IsTrue(form.Visible, "Form should remain open after dialog.");
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── View Auto-Backups (no backups present) ────────────────────────────

        [TestMethod]
        public void SettingsForm_ViewAutoBackups_WhenNoneExist_ShowsNoBackupsDialog()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var settings = new SettingsRepository(tempDir);

                    using (var form = new SettingsForm(settings, tempDir))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        using (new WinFormsTestHelper.DialogAutoCloser("Auto-Backups"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "View Auto-Backups").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                        Assert.IsTrue(form.Visible);
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── View Auto-Backups (backups present) ───────────────────────────────

        [TestMethod]
        public void SettingsForm_ViewAutoBackups_WhenBackupsExist_ShowsListDialog()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var settings = new SettingsRepository(tempDir);

                    // Create a fake auto-backup so the "has backups" branch runs
                    string backupDir = Path.Combine(tempDir, "Backups");
                    Directory.CreateDirectory(backupDir);
                    File.WriteAllText(Path.Combine(backupDir, "orders_auto_20260101.db"), "fake");

                    using (var form = new SettingsForm(settings, tempDir))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        using (new WinFormsTestHelper.DialogAutoCloser("Auto-Backups"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "View Auto-Backups").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                        Assert.IsTrue(form.Visible);
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string CreateTempDir()
        {
            string dir = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "PizzaExpressSettings_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static void DeleteTempDir(string dir)
        {
            if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }
}
