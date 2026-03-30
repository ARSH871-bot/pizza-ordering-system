using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
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
        private readonly ISettingsRepository _settings;
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

        public SettingsForm(ISettingsRepository settings)
        {
            _settings = settings ?? throw new ArgumentNullException("settings");
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

            // ── Layout order (dock fills work bottom-up for DockStyle.Fill) ───
            Controls.Add(_grid);
            Controls.Add(_lblHint);
            Controls.Add(header);
            Controls.Add(btnPanel);
        }

        // ── Data ──────────────────────────────────────────────────────────────

        private void LoadSettings()
        {
            _grid.Rows.Clear();
            IReadOnlyList<SettingRow> rows = _settings.GetAll();
            foreach (var row in rows)
                _grid.Rows.Add(FriendlyName(row.Key), row.Value);

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
    }
}
