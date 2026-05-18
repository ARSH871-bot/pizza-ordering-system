using System.IO;
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

        // ── DataGridView cell-edit lambdas ────────────────────────────────────

        [TestMethod]
        public void SettingsForm_CellBeginEdit_AndCellEndEdit_ChangeBackColor()
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

                        var grid = WinFormsTestHelper.GetPrivateField<DataGridView>(form, "_grid");
                        Assert.IsNotNull(grid, "_grid field not found.");
                        Assert.IsTrue(grid.Rows.Count > 0, "Grid must have at least one row.");

                        // Select the Value cell (column index 1) of the first row
                        grid.CurrentCell = grid.Rows[0].Cells[1];
                        WinFormsTestHelper.PumpEvents();

                        // BeginEdit triggers CellBeginEdit lambda — highlights the cell
                        grid.BeginEdit(true);
                        WinFormsTestHelper.PumpEvents();

                        // EndEdit triggers CellEndEdit lambda — resets the highlight
                        grid.EndEdit();
                        WinFormsTestHelper.PumpEvents();

                        // Cell background should be cleared
                        Assert.AreEqual(
                            System.Drawing.Color.Empty,
                            grid.Rows[0].Cells[1].Style.BackColor,
                            "CellEndEdit should reset cell BackColor to Empty.");
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── Save button paths ─────────────────────────────────────────────────

        [TestMethod]
        public void SettingsForm_SaveButton_WithValidData_ShowsSavedDialogAndCloses()
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

                        using (new WinFormsTestHelper.DialogAutoCloser("Saved"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "Save Changes").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                        Assert.IsFalse(form.Visible, "SettingsForm should close after successful save.");
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        [TestMethod]
        public void SettingsForm_SaveButton_WithInvalidNumericValue_ShowsValidationError()
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

                        var grid = WinFormsTestHelper.GetPrivateField<DataGridView>(form, "_grid");
                        foreach (DataGridViewRow row in grid.Rows)
                        {
                            string key = row.Tag as string;
                            if (SettingsForm.IsNumericKey(key))
                            {
                                row.Cells["Value"].Value = "notanumber";
                                break;
                            }
                        }

                        using (new WinFormsTestHelper.DialogAutoCloser("Validation Error"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "Save Changes").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                        Assert.IsTrue(form.Visible, "SettingsForm should remain open after validation error.");
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        // ── Restore button guards ─────────────────────────────────────────────

        [TestMethod]
        public void SettingsForm_RestoreButton_WithNoDataDir_ShowsBackupUnavailableDialog()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var settings = new SettingsRepository(tempDir);

                    using (var form = new SettingsForm(settings, dataDirectory: null))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        using (new WinFormsTestHelper.DialogAutoCloser("Backup Unavailable"))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "Restore DB").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                        Assert.IsTrue(form.Visible, "Form should remain open after unavailable dialog.");
                    }
                }
                finally { DeleteTempDir(tempDir); }
            });
        }

        [TestMethod]
        public void SettingsForm_RestoreButton_WithDataDir_NoConfirm_DoesNotRestore()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDir();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var settings = new SettingsRepository(tempDir);

                    using (var form = new SettingsForm(settings, dataDirectory: tempDir))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        // No PIN configured so EnsureAuthorized passes; confirmation dialog is
                        // dismissed with No so the restore is cancelled before the file dialog.
                        using (new WinFormsTestHelper.DialogAutoCloser("Restore Database", respondNo: true))
                            WinFormsTestHelper.FindByTextPrefix<Button>(form, "Restore DB").PerformClick();

                        WinFormsTestHelper.PumpEvents();
                        Assert.IsTrue(form.Visible, "Form should remain open after cancelled restore.");
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
