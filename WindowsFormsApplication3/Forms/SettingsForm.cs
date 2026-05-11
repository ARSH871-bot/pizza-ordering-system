using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using WindowsFormsApplication3.Infrastructure;
using WindowsFormsApplication3.Services;

namespace WindowsFormsApplication3.Forms
{
    /// <summary>
    /// Admin form for editing application settings stored in the SQLite
    /// <c>Settings</c> table.  Changes take effect immediately for the next
    /// order — no restart required.
    /// </summary>
    public class SettingsForm : Form
    {
        private const string StaffPinConfiguredPlaceholder = "configured (enter a new PIN to change, clear to disable)";

        private readonly ISettingsRepository _settings;
        private readonly string              _dataDirectory;
        private DataGridView _grid;
        private Button _btnSave;
        private Button _btnCancel;
        private Label _lblHint;

        // ── Design tokens (matches OrderHistoryForm dark theme) ───────────────
        private static readonly Color ClrBackground  = Color.FromArgb(26,  26,  26);
        private static readonly Color ClrSurface     = Color.FromArgb(38,  38,  38);
        private static readonly Color ClrBrand       = Color.FromArgb(200, 60,   0);
        private static readonly Color ClrNeutral     = Color.FromArgb(55,  55,  55);
        private static readonly Color ClrTextPrimary = Color.FromArgb(240, 240, 240);
        private static readonly Color ClrTextMuted   = Color.FromArgb(160, 160, 160);
        private static readonly Color ClrCellEdit    = Color.FromArgb(50,  50,  50);

        public SettingsForm(ISettingsRepository settings, string dataDirectory = null)
        {
            _settings      = settings ?? throw new ArgumentNullException("settings");
            _dataDirectory = dataDirectory;
            BuildUi();
            LoadSettings();
        }

        // ── UI construction ───────────────────────────────────────────────────

        private void BuildUi()
        {
            Text            = "Pizza Express — Settings";
            Size            = new Size(560, 560);
            MinimumSize     = new Size(440, 420);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = false;
            BackColor       = ClrBackground;
            ForeColor       = ClrTextPrimary;
            Font            = new Font("Segoe UI", 9.5f);

            // ── Header bar ────────────────────────────────────────────────────
            var header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 50,
                BackColor = Color.FromArgb(40, 20, 0),
            };
            var lblTitle = new Label
            {
                Text      = "Application Settings",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 200, 140),
                AutoSize  = true,
                Location  = new Point(14, 12),
            };
            header.Controls.Add(lblTitle);

            // ── Hint label ────────────────────────────────────────────────────
            _lblHint = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 30,
                Text      = "  Double-click a Value cell to edit.  Changes take effect on the next order.",
                ForeColor = ClrTextMuted,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = ClrSurface,
                Padding   = new Padding(6, 0, 0, 0),
            };

            // ── DataGridView ──────────────────────────────────────────────────
            _grid = new DataGridView
            {
                Dock                    = DockStyle.Fill,
                AllowUserToAddRows      = false,
                AllowUserToDeleteRows   = false,
                AllowUserToResizeRows   = false,
                RowHeadersVisible       = false,
                SelectionMode           = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode     = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor         = ClrBackground,
                GridColor               = Color.FromArgb(55, 55, 55),
                BorderStyle             = BorderStyle.None,
                CellBorderStyle         = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight     = 34,
                RowTemplate             = { Height = 30 },
                Font                    = new Font("Segoe UI", 9.5f),
            };

            // Column: Setting (read-only)
            var colKey = new DataGridViewTextBoxColumn
            {
                Name         = "Key",
                HeaderText   = "Setting",
                ReadOnly     = true,
                FillWeight   = 55,
                SortMode     = DataGridViewColumnSortMode.NotSortable,
            };

            // Column: Value (editable)
            var colVal = new DataGridViewTextBoxColumn
            {
                Name       = "Value",
                HeaderText = "Value",
                ReadOnly   = false,
                FillWeight = 45,
                SortMode   = DataGridViewColumnSortMode.NotSortable,
            };

            _grid.Columns.Add(colKey);
            _grid.Columns.Add(colVal);

            // ── Grid visual style ─────────────────────────────────────────────
            _grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor          = ClrBackground,
                ForeColor          = ClrTextPrimary,
                SelectionBackColor = Color.FromArgb(200, 60, 0),
                SelectionForeColor = Color.White,
                Padding            = new Padding(4, 0, 0, 0),
            };
            _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ClrSurface,
                ForeColor = ClrTextPrimary,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Padding   = new Padding(6, 0, 0, 0),
            };
            _grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor          = Color.FromArgb(32, 32, 32),
                ForeColor          = ClrTextPrimary,
                SelectionBackColor = Color.FromArgb(200, 60, 0),
                SelectionForeColor = Color.White,
            };

            // Highlight the editable Value column slightly
            _grid.CellBeginEdit += (s, e) =>
            {
                if (e.ColumnIndex == 1)
                    _grid.Rows[e.RowIndex].Cells[1].Style.BackColor = ClrCellEdit;
            };
            _grid.CellEndEdit += (s, e) =>
            {
                _grid.Rows[e.RowIndex].Cells[1].Style.BackColor = Color.Empty;
            };

            // ── Button bar ────────────────────────────────────────────────────
            _btnSave = new Button
            {
                Text   = "Save Changes",
                Width  = 130,
                Height = 34,
            };
            _btnSave.Click += BtnSave_Click;
            ApplyButtonStyle(_btnSave, ClrBrand, Color.White);

            _btnCancel = new Button
            {
                Text         = "Cancel",
                Width        = 90,
                Height       = 34,
                DialogResult = DialogResult.Cancel,
            };
            ApplyButtonStyle(_btnCancel, ClrNeutral, Color.White);
            _btnCancel.Click += (s, e) => Close();

            var btnPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 50,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(6),
                BackColor     = ClrSurface,
            };
            btnPanel.Controls.Add(_btnCancel);
            btnPanel.Controls.Add(_btnSave);

            // ── Backup / Restore panel ────────────────────────────────────────
            var btnBackup = new Button { Text = "Backup DB…", Width = 110, Height = 30 };
            ApplyButtonStyle(btnBackup, Color.FromArgb(30, 100, 50), Color.White);
            btnBackup.Click += BtnBackup_Click;

            var btnRestore = new Button { Text = "Restore DB…", Width = 110, Height = 30 };
            ApplyButtonStyle(btnRestore, Color.FromArgb(120, 60, 0), Color.White);
            btnRestore.Click += BtnRestore_Click;

            var btnViewBackups = new Button { Text = "View Auto-Backups", Width = 140, Height = 30 };
            ApplyButtonStyle(btnViewBackups, ClrNeutral, Color.White);
            btnViewBackups.Click += BtnViewBackups_Click;

            var lblBackupInfo = new Label
            {
                AutoSize  = false,
                Width     = 140,
                Height    = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ClrTextMuted,
                Font      = new Font("Segoe UI", 8f, FontStyle.Italic),
                Tag       = "dbsize",
            };

            var backupPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 42,
                FlowDirection = FlowDirection.LeftToRight,
                Padding       = new Padding(6, 5, 6, 0),
                BackColor     = Color.FromArgb(30, 30, 30),
            };
            backupPanel.Controls.Add(btnBackup);
            backupPanel.Controls.Add(btnRestore);
            backupPanel.Controls.Add(btnViewBackups);
            backupPanel.Controls.Add(lblBackupInfo);

            // ── Layout order (dock fills work bottom-up for DockStyle.Fill) ───
            Controls.Add(_grid);
            Controls.Add(_lblHint);
            Controls.Add(header);
            Controls.Add(backupPanel);
            Controls.Add(btnPanel);

            // Populate DB size label after layout
            Load += (s, e) => RefreshDbSizeLabel(lblBackupInfo);
        }

        // ── Data ──────────────────────────────────────────────────────────────

        private void LoadSettings()
        {
            _grid.Rows.Clear();
            IReadOnlyList<SettingRow> rows = _settings.GetAll();
            foreach (var row in rows)
                _grid.Rows.Add(FriendlyName(row.Key), GetDisplayValue(row));

            // Store original keys as row tags for saving back
            IReadOnlyList<SettingRow> allRows = _settings.GetAll();
            for (int i = 0; i < _grid.Rows.Count && i < allRows.Count; i++)
                _grid.Rows[i].Tag = allRows[i].Key;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var errors = new System.Text.StringBuilder();

            foreach (DataGridViewRow row in _grid.Rows)
            {
                string key   = row.Tag as string;
                string value = row.Cells["Value"].Value as string ?? string.Empty;
                value = value.Trim();

                if (string.IsNullOrEmpty(key)) continue;

                if (string.Equals(key, "StaffPin", StringComparison.Ordinal))
                {
                    if (!TrySaveStaffPin(value, errors))
                        continue;

                    continue;
                }

                // Validate: numeric keys must be parseable as a positive decimal
                if (IsNumericKey(key))
                {
                    decimal num;
                    if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out num)
                        || num < 0)
                    {
                        errors.AppendLine($"  {FriendlyName(key)}: must be a non-negative number (e.g. 4.00)");
                        continue;
                    }
                    value = num.ToString("F2", CultureInfo.InvariantCulture);
                }

                _settings.Set(key, value);
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(
                    "The following values could not be saved:\n\n" + errors.ToString() +
                    "\nAll other changes have been saved.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                LoadSettings();
                return;
            }

            MessageBox.Show(
                "Settings saved successfully.\nPrices take effect on the next order.",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            Close();
        }

        private string GetDisplayValue(SettingRow row)
        {
            if (row == null)
                return string.Empty;

            if (!string.Equals(row.Key, "StaffPin", StringComparison.Ordinal))
                return row.Value;

            return PinSecurity.IsConfigured(row.Value)
                ? StaffPinConfiguredPlaceholder
                : string.Empty;
        }

        private bool TrySaveStaffPin(string enteredValue, System.Text.StringBuilder errors)
        {
            string existingStoredPin = _settings.Get("StaffPin", string.Empty) ?? string.Empty;
            string candidate = (enteredValue ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(candidate))
            {
                _settings.Set("StaffPin", string.Empty);
                return true;
            }

            if (string.Equals(candidate, StaffPinConfiguredPlaceholder, StringComparison.Ordinal))
            {
                if (!PinSecurity.IsConfigured(existingStoredPin))
                {
                    _settings.Set("StaffPin", string.Empty);
                    return true;
                }

                if (!PinSecurity.IsProtected(existingStoredPin))
                    _settings.Set("StaffPin", PinSecurity.Protect(existingStoredPin));

                return true;
            }

            ValidationResult validation = PinSecurity.ValidateNewPin(candidate);
            if (!validation.IsValid)
            {
                errors.AppendLine($"  {FriendlyName("StaffPin")}: {validation.ErrorMessage}");
                return false;
            }

            _settings.Set("StaffPin", PinSecurity.Protect(candidate));
            return true;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string FriendlyName(string key)
        {
            switch (key)
            {
                case "PizzaPrice.Small":       return "Pizza Price — Small ($)";
                case "PizzaPrice.Medium":      return "Pizza Price — Medium ($)";
                case "PizzaPrice.Large":       return "Pizza Price — Large ($)";
                case "PizzaPrice.ExtraLarge":  return "Pizza Price — Extra Large ($)";
                case "ToppingPrice":           return "Topping Price ($)";
                case "DrinkCanPrice":          return "Drink Can Price ($)";
                case "WaterPrice":             return "Water Price ($)";
                case "SidePrice":              return "Side Item Price ($)";
                case "DeliveryMinutes":        return "Delivery Estimate (minutes)";
                case "StaffPin":               return "Staff PIN (leave blank to disable)";
                default:                       return key;
            }
        }

        private static bool IsNumericKey(string key)
        {
            return key == "PizzaPrice.Small"      || key == "PizzaPrice.Medium"  ||
                   key == "PizzaPrice.Large"       || key == "PizzaPrice.ExtraLarge" ||
                   key == "ToppingPrice"           || key == "DrinkCanPrice"     ||
                   key == "WaterPrice"             || key == "SidePrice"         ||
                   key == "DeliveryMinutes";
        }

        private static void ApplyButtonStyle(Button btn, Color back, Color fore)
        {
            btn.FlatStyle                 = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor                 = back;
            btn.ForeColor                 = fore;
            btn.Font                      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btn.Cursor                    = Cursors.Hand;
            btn.UseVisualStyleBackColor   = false;
        }

        // ── Backup / Restore handlers ─────────────────────────────────────────

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            if (_dataDirectory == null) { ShowNoDataDir(); return; }

            using (var dlg = new SaveFileDialog
            {
                Title            = "Save Database Backup",
                Filter           = "SQLite database (*.db)|*.db",
                FileName         = $"orders_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db",
                DefaultExt       = "db",
                OverwritePrompt  = true,
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    DatabaseBackupService.BackupTo(_dataDirectory, dlg.FileName);
                    MessageBox.Show(
                        $"Backup saved to:\n{dlg.FileName}",
                        "Backup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Backup failed:\n{ex.Message}", "Backup Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            if (_dataDirectory == null) { ShowNoDataDir(); return; }

            var confirm = MessageBox.Show(
                "Restoring a backup will replace the current database.\n" +
                "A safety copy of the current data will be saved first.\n\n" +
                "Continue?",
                "Restore Database",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            using (var dlg = new OpenFileDialog
            {
                Title  = "Select Backup to Restore",
                Filter = "SQLite database (*.db)|*.db|All files (*.*)|*.*",
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    string safetyPath = DatabaseBackupService.RestoreFrom(_dataDirectory, dlg.FileName);
                    MessageBox.Show(
                        $"Database restored successfully.\n\n" +
                        $"Safety copy of previous data:\n{safetyPath}\n\n" +
                        "Please restart the application for changes to take full effect.",
                        "Restore Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Restore failed:\n{ex.Message}", "Restore Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnViewBackups_Click(object sender, EventArgs e)
        {
            if (_dataDirectory == null) { ShowNoDataDir(); return; }

            string[] backups = DatabaseBackupService.GetAutoBackups(_dataDirectory);
            if (backups.Length == 0)
            {
                MessageBox.Show(
                    "No auto-backups found yet.\nAuto-backups are created once per day on startup.",
                    "Auto-Backups", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Auto-backups ({backups.Length}):\n");
            foreach (string p in backups)
                sb.AppendLine($"  {Path.GetFileName(p)}");
            sb.AppendLine($"\nFolder:\n  {Path.GetDirectoryName(backups[0])}");

            MessageBox.Show(sb.ToString(), "Auto-Backups",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RefreshDbSizeLabel(Label lbl)
        {
            if (_dataDirectory == null) return;
            long kb = DatabaseBackupService.GetDatabaseSizeKb(_dataDirectory);
            lbl.Text = kb > 0 ? $"DB: {kb} KB" : "DB: —";
        }

        private void ShowNoDataDir()
        {
            MessageBox.Show(
                "Data directory is not available in this context.",
                "Backup Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
