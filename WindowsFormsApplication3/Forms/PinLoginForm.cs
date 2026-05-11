using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApplication3.Services;

namespace WindowsFormsApplication3.Forms
{
    /// <summary>
    /// Staff PIN entry dialog shown at application startup and reused for
    /// sensitive actions when needed.
    /// </summary>
    public class PinLoginForm : Form
    {
        private const int MaxFailedAttemptsBeforeLockout = 3;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromSeconds(10);

        private static readonly Color ClrBg = Color.FromArgb(26, 26, 26);
        private static readonly Color ClrSurface = Color.FromArgb(38, 38, 38);
        private static readonly Color ClrBrand = Color.FromArgb(200, 60, 0);
        private static readonly Color ClrNeutral = Color.FromArgb(55, 55, 55);
        private static readonly Color ClrText = Color.FromArgb(240, 240, 240);
        private static readonly Color ClrMuted = Color.FromArgb(160, 160, 160);
        private static readonly Color ClrAmber = Color.FromArgb(255, 200, 140);
        private static readonly Color ClrError = Color.FromArgb(255, 100, 100);
        private static readonly Color ClrDanger = Color.FromArgb(160, 30, 30);

        private readonly ISettingsRepository _settings;
        private string _storedPin;
        private string _entered = string.Empty;
        private int _failedAttempts;
        private DateTime _lockedUntilUtc = DateTime.MinValue;

        private Label _lblDots;
        private Label _lblError;
        private Button _btnEnter;
        private System.Windows.Forms.Timer _lockoutTimer;

        public static bool PinRequired(ISettingsRepository settings)
        {
            string pin = settings?.Get("StaffPin", string.Empty) ?? string.Empty;
            return PinSecurity.IsConfigured(pin);
        }

        public static bool EnsureAuthorized(
            IWin32Window owner,
            ISettingsRepository settings,
            TimeSpan maxAge)
        {
            if (!PinRequired(settings))
                return true;

            if (StaffAuthSession.HasRecentAuthorization(maxAge))
                return true;

            using (var login = new PinLoginForm(settings))
                return login.ShowDialog(owner) == DialogResult.OK;
        }

        public PinLoginForm(ISettingsRepository settings)
        {
            _settings = settings;
            _storedPin = settings?.Get("StaffPin", string.Empty) ?? string.Empty;
            BuildUi();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _lockoutTimer != null)
            {
                _lockoutTimer.Stop();
                _lockoutTimer.Dispose();
                _lockoutTimer = null;
            }

            base.Dispose(disposing);
        }

        private void BuildUi()
        {
            Text = "Pizza Express - Staff Login";
            Size = new Size(340, 520);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = ClrBg;
            ForeColor = ClrText;
            Font = new Font("Segoe UI", 10f);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(40, 20, 0),
            };
            header.Controls.Add(new Label
            {
                Text = "Staff Login",
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = ClrAmber,
                AutoSize = true,
                Location = new Point(14, 16),
            });

            var pinArea = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = ClrSurface,
                Padding = new Padding(0, 14, 0, 0),
            };
            _lblDots = new Label
            {
                Text = string.Empty,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = ClrAmber,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            _lblError = new Label
            {
                Text = string.Empty,
                Dock = DockStyle.Bottom,
                Height = 20,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = ClrError,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = ClrSurface,
            };
            pinArea.Controls.Add(_lblDots);
            pinArea.Controls.Add(_lblError);

            var padPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 5,
                BackColor = ClrBg,
                Padding = new Padding(18, 12, 18, 8),
            };
            for (int i = 0; i < 3; i++)
                padPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            for (int i = 0; i < 5; i++)
                padPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

            int[] digits = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            for (int i = 0; i < digits.Length; i++)
            {
                int digit = digits[i];
                var btn = MakeNumButton(digit.ToString());
                btn.Click += (s, e) => AppendDigit(digit.ToString());
                padPanel.Controls.Add(btn, i % 3, i / 3);
            }

            var btnClear = MakeNumButton("CLR", ClrDanger);
            btnClear.Click += (s, e) =>
            {
                if (IsLockedOut())
                    return;

                _entered = string.Empty;
                UpdateDots();
            };

            var btnZero = MakeNumButton("0");
            btnZero.Click += (s, e) => AppendDigit("0");

            var btnBack = MakeNumButton("Back", ClrNeutral);
            btnBack.Click += (s, e) =>
            {
                if (IsLockedOut())
                    return;

                if (_entered.Length > 0)
                    _entered = _entered.Substring(0, _entered.Length - 1);

                UpdateDots();
            };

            padPanel.Controls.Add(btnClear, 0, 3);
            padPanel.Controls.Add(btnZero, 1, 3);
            padPanel.Controls.Add(btnBack, 2, 3);

            _btnEnter = new Button
            {
                Text = "Unlock",
                Dock = DockStyle.Fill,
                Margin = new Padding(4),
            };
            ApplyBtnStyle(_btnEnter, ClrBrand, Color.White, 12f, FontStyle.Bold);
            _btnEnter.Click += BtnEnter_Click;
            padPanel.Controls.Add(_btnEnter, 0, 4);
            padPanel.SetColumnSpan(_btnEnter, 3);

            var lblHint = new Label
            {
                Text = "Enter your staff PIN to continue",
                Dock = DockStyle.Bottom,
                Height = 28,
                ForeColor = ClrMuted,
                Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = ClrBg,
            };

            Controls.Add(padPanel);
            Controls.Add(pinArea);
            Controls.Add(header);
            Controls.Add(lblHint);

            KeyPreview = true;
            KeyDown += OnKeyDown;
        }

        private void AppendDigit(string digit)
        {
            if (IsLockedOut())
                return;

            if (_entered.Length >= 12)
                return;

            _entered += digit;
            _lblError.Text = string.Empty;
            UpdateDots();
        }

        private void UpdateDots()
        {
            _lblDots.Text = new string('\u25CF', _entered.Length);
        }

        private void BtnEnter_Click(object sender, EventArgs e)
        {
            if (IsLockedOut())
            {
                UpdateLockoutMessage();
                return;
            }

            if (PinSecurity.Verify(_entered, _storedPin))
            {
                UpgradeLegacyPinIfNeeded();
                StaffAuthSession.MarkAuthenticated();
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            _failedAttempts++;
            _entered = string.Empty;
            UpdateDots();

            if (_failedAttempts >= MaxFailedAttemptsBeforeLockout)
            {
                StartLockout();
                return;
            }

            int remainingAttempts = MaxFailedAttemptsBeforeLockout - _failedAttempts;
            _lblError.Text = remainingAttempts == 1
                ? "Incorrect PIN. 1 attempt remaining."
                : string.Format("Incorrect PIN. {0} attempts remaining.", remainingAttempts);
            ShakeForm();
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
                if (IsLockedOut())
                {
                    e.Handled = true;
                    return;
                }

                if (_entered.Length > 0)
                    _entered = _entered.Substring(0, _entered.Length - 1);

                UpdateDots();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (IsLockedOut())
                {
                    e.Handled = true;
                    return;
                }

                _entered = string.Empty;
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
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        private void StartLockout()
        {
            _lockedUntilUtc = DateTime.UtcNow.Add(LockoutDuration);
            _btnEnter.Enabled = false;
            UpdateLockoutMessage();
            EnsureLockoutTimer();
            ShakeForm();
        }

        private bool IsLockedOut()
        {
            if (_lockedUntilUtc == DateTime.MinValue)
                return false;

            if (DateTime.UtcNow >= _lockedUntilUtc)
            {
                ResetLockout();
                return false;
            }

            return true;
        }

        private void ResetLockout()
        {
            _failedAttempts = 0;
            _lockedUntilUtc = DateTime.MinValue;
            _btnEnter.Enabled = true;
            _lblError.Text = string.Empty;
        }

        private void EnsureLockoutTimer()
        {
            if (_lockoutTimer != null)
            {
                _lockoutTimer.Start();
                return;
            }

            _lockoutTimer = new System.Windows.Forms.Timer { Interval = 250 };
            _lockoutTimer.Tick += (s, e) =>
            {
                if (IsLockedOut())
                {
                    UpdateLockoutMessage();
                    return;
                }

                _lockoutTimer.Stop();
            };
            _lockoutTimer.Start();
        }

        private void UpdateLockoutMessage()
        {
            TimeSpan remaining = _lockedUntilUtc - DateTime.UtcNow;
            if (remaining < TimeSpan.Zero)
                remaining = TimeSpan.Zero;

            int secondsRemaining = Math.Max(1, (int)Math.Ceiling(remaining.TotalSeconds));
            _lblError.Text = string.Format(
                "Too many incorrect attempts. Try again in {0}s.",
                secondsRemaining);
        }

        private void UpgradeLegacyPinIfNeeded()
        {
            if (_settings == null)
                return;

            if (!PinSecurity.IsConfigured(_storedPin) || PinSecurity.IsProtected(_storedPin))
                return;

            string protectedPin = PinSecurity.Protect(_entered);
            _settings.Set("StaffPin", protectedPin);
            _storedPin = protectedPin;
        }

        private void ShakeForm()
        {
            int originX = Left;
            var timer = new System.Windows.Forms.Timer { Interval = 40 };
            int ticks = 0;
            int[] offsets = { -8, 8, -6, 6, -4, 4, -2, 2, 0 };
            timer.Tick += (s, e) =>
            {
                if (ticks < offsets.Length)
                {
                    Left = originX + offsets[ticks++];
                    return;
                }

                Left = originX;
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        private Button MakeNumButton(string label, Color? back = null)
        {
            var btn = new Button
            {
                Text = label,
                Dock = DockStyle.Fill,
                Margin = new Padding(4),
            };
            ApplyBtnStyle(btn, back ?? ClrNeutral, ClrText, 14f, FontStyle.Bold);
            return btn;
        }

        private static void ApplyBtnStyle(
            Button btn,
            Color back,
            Color fore,
            float fontSize = 10f,
            FontStyle style = FontStyle.Regular)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = back;
            btn.ForeColor = fore;
            btn.Font = new Font("Segoe UI", fontSize, style);
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
        }
    }
}
