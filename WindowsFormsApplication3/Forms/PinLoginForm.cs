using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApplication3.Services;

namespace WindowsFormsApplication3.Forms
{
    /// <summary>
    /// Staff PIN entry dialog shown at application startup (and optionally before
    /// sensitive actions).  If the stored <c>StaffPin</c> setting is empty the
    /// dialog is bypassed — the caller should check <see cref="PinRequired"/>
    /// before showing the form.
    /// </summary>
    public class PinLoginForm : Form
    {
        // ── Design tokens ────────────────────────────────────────────────────
        private static readonly Color ClrBg      = Color.FromArgb(26,  26,  26);
        private static readonly Color ClrSurface = Color.FromArgb(38,  38,  38);
        private static readonly Color ClrBrand   = Color.FromArgb(200, 60,   0);
        private static readonly Color ClrNeutral = Color.FromArgb(55,  55,  55);
        private static readonly Color ClrText    = Color.FromArgb(240, 240, 240);
        private static readonly Color ClrMuted   = Color.FromArgb(160, 160, 160);
        private static readonly Color ClrAmber   = Color.FromArgb(255, 200, 140);
        private static readonly Color ClrError   = Color.FromArgb(255, 100, 100);
        private static readonly Color ClrDanger  = Color.FromArgb(160, 30,  30);

        private readonly string _correctPin;
        private string _entered = string.Empty;

        // Controls
        private Label   _lblDots;
        private Label   _lblError;
        private Button  _btnEnter;

        /// <summary>
        /// Returns <c>true</c> if a non-empty PIN has been stored, meaning the
        /// form must be shown before granting access.
        /// </summary>
        public static bool PinRequired(ISettingsRepository settings)
        {
            string pin = settings?.Get("StaffPin", "") ?? "";
            return !string.IsNullOrWhiteSpace(pin);
        }

        /// <param name="settings">Repository used to read the stored PIN.</param>
        public PinLoginForm(ISettingsRepository settings)
        {
            _correctPin = settings?.Get("StaffPin", "") ?? "";
            BuildUi();
        }

        // ── UI construction ──────────────────────────────────────────────────

        private void BuildUi()
        {
            Text            = "Pizza Express — Staff Login";
            Size            = new Size(340, 520);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = ClrBg;
            ForeColor       = ClrText;
            Font            = new Font("Segoe UI", 10f);

            // ── Header ────────────────────────────────────────────────────────
            var header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 60,
                BackColor = Color.FromArgb(40, 20, 0),
            };
            header.Controls.Add(new Label
            {
                Text      = "Staff Login",
                Font      = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = ClrAmber,
                AutoSize  = true,
                Location  = new Point(14, 16),
            });

            // ── PIN display dots ─────────────────────────────────────────────
            var pinArea = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 70,
                BackColor = ClrSurface,
                Padding   = new Padding(0, 14, 0, 0),
            };
            _lblDots = new Label
            {
                Text      = "",
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = ClrAmber,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            _lblError = new Label
            {
                Text      = "",
                Dock      = DockStyle.Bottom,
                Height    = 20,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = ClrError,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = ClrSurface,
            };
            pinArea.Controls.Add(_lblDots);
            pinArea.Controls.Add(_lblError);

            // ── Numpad ───────────────────────────────────────────────────────
            var padPanel = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 3,
                RowCount    = 5,
                BackColor   = ClrBg,
                Padding     = new Padding(18, 12, 18, 8),
            };
            for (int i = 0; i < 3; i++)
                padPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            for (int i = 0; i < 5; i++)
                padPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

            // Rows 1-3: digits 1-9
            int[] digits = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            for (int i = 0; i < 9; i++)
            {
                int d = digits[i];
                var btn = MakeNumButton(d.ToString());
                btn.Click += (s, e) => AppendDigit(d.ToString());
                padPanel.Controls.Add(btn, i % 3, i / 3);
            }

            // Row 4: Clear | 0 | Back
            var btnClear = MakeNumButton("CLR", ClrDanger);
            btnClear.Click += (s, e) => { _entered = ""; UpdateDots(); };

            var btn0 = MakeNumButton("0");
            btn0.Click += (s, e) => AppendDigit("0");

            var btnBack = MakeNumButton("\u232B", ClrNeutral); // ⌫
            btnBack.Click += (s, e) =>
            {
                if (_entered.Length > 0)
                    _entered = _entered.Substring(0, _entered.Length - 1);
                UpdateDots();
            };

            padPanel.Controls.Add(btnClear, 0, 3);
            padPanel.Controls.Add(btn0,     1, 3);
            padPanel.Controls.Add(btnBack,  2, 3);

            // Row 5: Enter (spans all 3 columns)
            _btnEnter = new Button
            {
                Text   = "Unlock",
                Dock   = DockStyle.Fill,
                Margin = new Padding(4, 4, 4, 4),
            };
            ApplyBtnStyle(_btnEnter, ClrBrand, Color.White, 12f, FontStyle.Bold);
            _btnEnter.Click += BtnEnter_Click;
            padPanel.Controls.Add(_btnEnter, 0, 4);
            padPanel.SetColumnSpan(_btnEnter, 3);

            // ── Footer hint ──────────────────────────────────────────────────
            var lblHint = new Label
            {
                Text      = "Enter your staff PIN to continue",
                Dock      = DockStyle.Bottom,
                Height    = 28,
                ForeColor = ClrMuted,
                Font      = new Font("Segoe UI", 8f, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = ClrBg,
            };

            Controls.Add(padPanel);
            Controls.Add(pinArea);
            Controls.Add(header);
            Controls.Add(lblHint);

            // Support keyboard input
            KeyPreview = true;
            KeyDown   += OnKeyDown;
        }

        // ── Interaction ──────────────────────────────────────────────────────

        private void AppendDigit(string digit)
        {
            if (_entered.Length >= 12) return;   // cap at 12 digits
            _entered += digit;
            _lblError.Text = "";
            UpdateDots();
        }

        private void UpdateDots()
        {
            // Show filled circles proportional to digits entered
            _lblDots.Text = new string('\u25CF', _entered.Length);   // ●
        }

        private void BtnEnter_Click(object sender, EventArgs e)
        {
            if (_entered == _correctPin)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _lblError.Text = "Incorrect PIN — try again";
                _entered       = "";
                UpdateDots();

                // Brief shake animation
                ShakeForm();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
            {
                AppendDigit(((int)(e.KeyCode - Keys.D0)).ToString());
                e.Handled = true;
            }
            else if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
            {
                AppendDigit(((int)(e.KeyCode - Keys.NumPad0)).ToString());
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Back)
            {
                if (_entered.Length > 0)
                    _entered = _entered.Substring(0, _entered.Length - 1);
                UpdateDots();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                _entered = "";
                UpdateDots();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                BtnEnter_Click(this, EventArgs.Empty);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // Close app if user cancels login
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        // ── Shake animation ──────────────────────────────────────────────────

        private void ShakeForm()
        {
            int originX = Left;
            var timer   = new System.Windows.Forms.Timer { Interval = 40 };
            int ticks   = 0;
            int[] offsets = { -8, 8, -6, 6, -4, 4, -2, 2, 0 };
            timer.Tick += (s, e) =>
            {
                if (ticks < offsets.Length)
                    Left = originX + offsets[ticks++];
                else
                {
                    Left = originX;
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private Button MakeNumButton(string label, Color? back = null)
        {
            var btn = new Button
            {
                Text   = label,
                Dock   = DockStyle.Fill,
                Margin = new Padding(4, 4, 4, 4),
            };
            ApplyBtnStyle(btn, back ?? ClrNeutral, ClrText, 14f, FontStyle.Bold);
            return btn;
        }

        private static void ApplyBtnStyle(Button btn, Color back, Color fore,
                                          float fontSize = 10f, FontStyle style = FontStyle.Regular)
        {
            btn.FlatStyle                 = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor                 = back;
            btn.ForeColor                 = fore;
            btn.Font                      = new Font("Segoe UI", fontSize, style);
            btn.Cursor                    = Cursors.Hand;
            btn.UseVisualStyleBackColor   = false;
        }
    }
}
