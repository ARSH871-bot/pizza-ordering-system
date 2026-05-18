using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Forms;
using WindowsFormsApplication3.Infrastructure;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    [DoNotParallelize]
    [TestClass]
    public class PinLoginFormKeyboardTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static Label GetDotsLabel(PinLoginForm form)
            => WinFormsTestHelper.GetPrivateField<Label>(form, "_lblDots");

        private static string CreateTempDataDirectory()
        {
            string dir = Path.Combine(Path.GetTempPath(), "PizzaExpressKbd_" + Guid.NewGuid().ToString("N"));
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

        // ── Digit and numpad keys ─────────────────────────────────────────────

        [TestMethod]
        public void OnKeyDown_DigitKey_AppendsOneDot()
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

                        var e = new KeyEventArgs(Keys.D3);
                        form.OnKeyDown(form, e);

                        Assert.AreEqual("●", GetDotsLabel(form).Text,
                            "One dot should appear after pressing a digit key.");
                        Assert.IsTrue(e.Handled, "Key should be marked handled.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void OnKeyDown_NumPadKey_AppendsOneDot()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var settings = new SettingsRepository(tempDir);
                    settings.Set("StaffPin", PinSecurity.Protect("5678"));

                    using (var form = new PinLoginForm(settings))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        var e = new KeyEventArgs(Keys.NumPad5);
                        form.OnKeyDown(form, e);

                        Assert.AreEqual("●", GetDotsLabel(form).Text,
                            "One dot should appear after pressing a numpad key.");
                        Assert.IsTrue(e.Handled);
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        // ── Backspace and Delete ──────────────────────────────────────────────

        [TestMethod]
        public void OnKeyDown_Backspace_RemovesLastDigit()
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

                        form.OnKeyDown(form, new KeyEventArgs(Keys.D1));
                        form.OnKeyDown(form, new KeyEventArgs(Keys.D2));
                        Assert.AreEqual("●●", GetDotsLabel(form).Text);

                        var backE = new KeyEventArgs(Keys.Back);
                        form.OnKeyDown(form, backE);

                        Assert.AreEqual("●", GetDotsLabel(form).Text,
                            "Backspace should remove the last digit.");
                        Assert.IsTrue(backE.Handled);
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void OnKeyDown_Delete_ClearsAllDigits()
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

                        form.OnKeyDown(form, new KeyEventArgs(Keys.D1));
                        form.OnKeyDown(form, new KeyEventArgs(Keys.D2));
                        form.OnKeyDown(form, new KeyEventArgs(Keys.D3));
                        Assert.AreEqual(3, GetDotsLabel(form).Text.Length);

                        var delE = new KeyEventArgs(Keys.Delete);
                        form.OnKeyDown(form, delE);

                        Assert.AreEqual(string.Empty, GetDotsLabel(form).Text,
                            "Delete key should clear all entered digits.");
                        Assert.IsTrue(delE.Handled);
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        // ── Enter and Escape ──────────────────────────────────────────────────

        [TestMethod]
        public void OnKeyDown_Enter_WithCorrectPin_ClosesWithOk()
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

                        foreach (char c in "1234")
                            form.OnKeyDown(form, new KeyEventArgs(Keys.D0 + (c - '0')));

                        var enterE = new KeyEventArgs(Keys.Enter);
                        form.OnKeyDown(form, enterE);
                        WinFormsTestHelper.PumpEvents();

                        Assert.AreEqual(DialogResult.OK, form.DialogResult,
                            "Enter with correct PIN should close with OK.");
                        Assert.IsTrue(enterE.Handled);
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
        public void OnKeyDown_Escape_ClosesWithCancel()
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

                        var escE = new KeyEventArgs(Keys.Escape);
                        form.OnKeyDown(form, escE);
                        WinFormsTestHelper.PumpEvents();

                        Assert.AreEqual(DialogResult.Cancel, form.DialogResult,
                            "Escape should close the form with Cancel result.");
                        Assert.IsTrue(escE.Handled);
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void OnKeyDown_UnhandledKey_LeavesHandledFalse()
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

                        var e = new KeyEventArgs(Keys.F5);
                        form.OnKeyDown(form, e);

                        Assert.IsFalse(e.Handled, "Non-handled key should leave Handled as false.");
                        Assert.AreEqual(string.Empty, GetDotsLabel(form).Text);
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        // ── Max-length guard ──────────────────────────────────────────────────

        [TestMethod]
        public void AppendDigit_AtMaxLength_DoesNotAddMore()
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

                        // Fill to 12-digit limit
                        for (int i = 0; i < 12; i++)
                            form.OnKeyDown(form, new KeyEventArgs(Keys.D1));

                        Assert.AreEqual(12, GetDotsLabel(form).Text.Length);

                        // 13th digit should be ignored
                        form.OnKeyDown(form, new KeyEventArgs(Keys.D2));

                        Assert.AreEqual(12, GetDotsLabel(form).Text.Length,
                            "PIN entry should stop at 12 digits.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        // ── Intermediate error messages (1 and 2 remaining attempts) ──────────

        [TestMethod]
        public void PinLoginForm_IncorrectPin_FirstAttempt_ShowsAttemptsRemainingMessage()
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

                        // Enter wrong PIN "9999" once
                        for (int i = 0; i < 4; i++)
                            form.OnKeyDown(form, new KeyEventArgs(Keys.D9));
                        form.OnKeyDown(form, new KeyEventArgs(Keys.Enter));
                        WinFormsTestHelper.PumpEvents();

                        Label error = WinFormsTestHelper.GetPrivateField<Label>(form, "_lblError");
                        StringAssert.Contains(error.Text, "2 attempts remaining",
                            "After first wrong attempt, should show 2 remaining.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void PinLoginForm_IncorrectPin_SecondAttempt_ShowsOneRemainingMessage()
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

                        // Enter wrong PIN "9999" twice
                        for (int attempt = 0; attempt < 2; attempt++)
                        {
                            for (int i = 0; i < 4; i++)
                                form.OnKeyDown(form, new KeyEventArgs(Keys.D9));
                            form.OnKeyDown(form, new KeyEventArgs(Keys.Enter));
                            WinFormsTestHelper.PumpEvents();
                        }

                        Label error = WinFormsTestHelper.GetPrivateField<Label>(form, "_lblError");
                        StringAssert.Contains(error.Text, "1 attempt remaining",
                            "After second wrong attempt, should show 1 remaining.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        // ── Locked-out guard paths ────────────────────────────────────────────

        [TestMethod]
        public void PinLoginForm_AppendDigit_WhileLockedOut_DoesNotChange()
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
                        TriggerLockout(form);

                        int dotsBefore = GetDotsLabel(form).Text.Length;

                        // Attempt to add a digit while locked out
                        form.OnKeyDown(form, new KeyEventArgs(Keys.D5));
                        WinFormsTestHelper.PumpEvents();

                        Assert.AreEqual(dotsBefore, GetDotsLabel(form).Text.Length,
                            "No digit should be appended while locked out.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void PinLoginForm_BtnEnter_WhileLockedOut_UpdatesLockoutMessage()
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
                        TriggerLockout(form);

                        // Click Unlock while locked out — should call UpdateLockoutMessage and return
                        var btnEnter = WinFormsTestHelper.GetPrivateField<Button>(form, "_btnEnter");
                        var btnEnterClick = typeof(PinLoginForm).GetMethod(
                            "BtnEnter_Click", BindingFlags.NonPublic | BindingFlags.Instance);
                        btnEnterClick.Invoke(form, new object[] { btnEnter, EventArgs.Empty });
                        WinFormsTestHelper.PumpEvents();

                        Label error = WinFormsTestHelper.GetPrivateField<Label>(form, "_lblError");
                        StringAssert.Contains(error.Text, "Try again",
                            "Locked-out click should update the lockout message.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void PinLoginForm_OnKeyDown_Backspace_WhileLockedOut_HandledNoChange()
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
                        TriggerLockout(form);

                        int dotsBefore = GetDotsLabel(form).Text.Length;

                        var backE = new KeyEventArgs(Keys.Back);
                        form.OnKeyDown(form, backE);

                        Assert.IsTrue(backE.Handled, "Backspace while locked out should be marked handled.");
                        Assert.AreEqual(dotsBefore, GetDotsLabel(form).Text.Length,
                            "Dots should not change on Backspace while locked out.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void PinLoginForm_OnKeyDown_Delete_WhileLockedOut_HandledNoChange()
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
                        TriggerLockout(form);

                        int dotsBefore = GetDotsLabel(form).Text.Length;

                        var delE = new KeyEventArgs(Keys.Delete);
                        form.OnKeyDown(form, delE);

                        Assert.IsTrue(delE.Handled, "Delete while locked out should be marked handled.");
                        Assert.AreEqual(dotsBefore, GetDotsLabel(form).Text.Length,
                            "Dots should not change on Delete while locked out.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        // ── Correct PIN — success path ────────────────────────────────────────

        [TestMethod]
        public void PinLoginForm_CorrectPin_ClosesWithDialogResultOk()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                string tempDir = CreateTempDataDirectory();
                try
                {
                    DatabaseMigrator.Run(tempDir);
                    var settings = new SettingsRepository(tempDir);

                    // Store a known PBKDF2-protected PIN
                    settings.Set("StaffPin", PinSecurity.Protect("1234"));

                    using (var form = new PinLoginForm(settings))
                    {
                        form.Show();
                        WinFormsTestHelper.PumpEvents();

                        // Enter "1234" via keyboard
                        foreach (var key in new[] { Keys.D1, Keys.D2, Keys.D3, Keys.D4 })
                            form.OnKeyDown(form, new KeyEventArgs(key));

                        // Submit
                        form.OnKeyDown(form, new KeyEventArgs(Keys.Enter));
                        WinFormsTestHelper.PumpEvents();

                        Assert.AreEqual(DialogResult.OK, form.DialogResult,
                            "Correct PIN should close the form with DialogResult.OK.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        // ── UpgradeLegacyPinIfNeeded with null settings ───────────────────────

        [TestMethod]
        public void PinLoginForm_UpgradeLegacyPin_NullSettings_DoesNotThrow()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                using (var form = new PinLoginForm(null))
                {
                    form.Show();
                    WinFormsTestHelper.PumpEvents();

                    // Invoke the private method directly — should return immediately without throwing
                    var mi = typeof(PinLoginForm).GetMethod(
                        "UpgradeLegacyPinIfNeeded", BindingFlags.NonPublic | BindingFlags.Instance);
                    mi.Invoke(form, null);
                }
            });
        }

        // ── EnsureLockoutTimer second lockout (timer already exists) ─────────

        [TestMethod]
        public void PinLoginForm_EnsureLockoutTimer_SecondLockout_ReusesTimer()
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

                        // First lockout — creates the timer (_lockoutTimer == null path in EnsureLockoutTimer)
                        TriggerLockout(form);

                        // Reset lockout state via reflection so a second lockout cycle can fire
                        var type   = typeof(PinLoginForm);
                        type.GetField("_lockedUntilUtc",  BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form, DateTime.MinValue);
                        type.GetField("_failedAttempts",  BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form, 0);
                        WinFormsTestHelper.GetPrivateField<Button>(form, "_btnEnter").Enabled = true;

                        // Second lockout — _lockoutTimer != null, so EnsureLockoutTimer restarts it (lines 352-354)
                        TriggerLockout(form);

                        // Verify form is still locked out (timer restart succeeded)
                        Assert.IsFalse(
                            WinFormsTestHelper.GetPrivateField<Button>(form, "_btnEnter").Enabled,
                            "Enter button should be disabled after second lockout.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        // ── LockoutTimer stops when lockout expires ───────────────────────────

        [TestMethod]
        public void PinLoginForm_LockoutTimer_AfterExpiry_Stops()
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
                        TriggerLockout(form);

                        // Back-date the lockout so it appears expired
                        typeof(PinLoginForm)
                            .GetField("_lockedUntilUtc", BindingFlags.NonPublic | BindingFlags.Instance)
                            .SetValue(form, DateTime.UtcNow.AddSeconds(-1));

                        // Pump the message loop long enough for the 250 ms timer to tick and stop
                        Thread.Sleep(350);
                        WinFormsTestHelper.PumpEvents();
                        Thread.Sleep(150);
                        WinFormsTestHelper.PumpEvents();

                        var timer = WinFormsTestHelper.GetPrivateField<System.Windows.Forms.Timer>(
                            form, "_lockoutTimer");
                        Assert.IsFalse(timer.Enabled,
                            "Lockout timer should stop once the lockout period expires.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        // ── UpdateLockoutMessage clamp path ───────────────────────────────────

        [TestMethod]
        public void PinLoginForm_UpdateLockoutMessage_AfterExpiry_ClampsToZero()
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
                        TriggerLockout(form);

                        // Back-date so remaining time is negative
                        typeof(PinLoginForm)
                            .GetField("_lockedUntilUtc", BindingFlags.NonPublic | BindingFlags.Instance)
                            .SetValue(form, DateTime.UtcNow.AddSeconds(-5));

                        // Call UpdateLockoutMessage directly — remaining is negative, should clamp to 0
                        typeof(PinLoginForm)
                            .GetMethod("UpdateLockoutMessage", BindingFlags.NonPublic | BindingFlags.Instance)
                            .Invoke(form, null);

                        var lblError = WinFormsTestHelper.GetPrivateField<Label>(form, "_lblError");
                        StringAssert.Contains(lblError.Text, "1s",
                            "Clamped remaining should display as 1 second (Math.Max(1, ...)).");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        // ── IsLockedOut expiry path (ResetLockout) ────────────────────────────

        [TestMethod]
        public void PinLoginForm_IsLockedOut_AfterExpiry_CallsResetLockout()
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
                        TriggerLockout(form);

                        // Force lockout to appear expired via reflection
                        typeof(PinLoginForm)
                            .GetField("_lockedUntilUtc", BindingFlags.NonPublic | BindingFlags.Instance)
                            .SetValue(form, DateTime.UtcNow.AddSeconds(-1));

                        // Calling IsLockedOut now hits the expiry branch and invokes ResetLockout
                        var isLockedOut = typeof(PinLoginForm).GetMethod(
                            "IsLockedOut", BindingFlags.NonPublic | BindingFlags.Instance);
                        bool result = (bool)isLockedOut.Invoke(form, null);

                        Assert.IsFalse(result, "IsLockedOut should return false once the lockout time has passed.");
                        Assert.IsTrue(
                            WinFormsTestHelper.GetPrivateField<Button>(form, "_btnEnter").Enabled,
                            "Enter button should be re-enabled after lockout expires.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        // ── CLR and Back buttons while locked out ─────────────────────────────

        [TestMethod]
        public void PinLoginForm_ClearButton_WhileLockedOut_DoesNothing()
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
                        TriggerLockout(form);

                        int dotsBefore = GetDotsLabel(form).Text.Length;
                        WinFormsTestHelper.FindByTextPrefix<Button>(form, "CLR").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        Assert.AreEqual(dotsBefore, GetDotsLabel(form).Text.Length,
                            "CLR should have no effect while the form is locked out.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void PinLoginForm_BackButton_WhileLockedOut_DoesNothing()
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
                        TriggerLockout(form);

                        int dotsBefore = GetDotsLabel(form).Text.Length;
                        WinFormsTestHelper.FindByTextPrefix<Button>(form, "Back").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        Assert.AreEqual(dotsBefore, GetDotsLabel(form).Text.Length,
                            "Back should have no effect while the form is locked out.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        // ── CLR and Back pad buttons ──────────────────────────────────────────

        [TestMethod]
        public void PinLoginForm_ClearButton_ClearsEnteredPin()
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

                        form.OnKeyDown(form, new KeyEventArgs(Keys.D1));
                        form.OnKeyDown(form, new KeyEventArgs(Keys.D2));
                        Assert.AreEqual(2, GetDotsLabel(form).Text.Length, "Should have 2 dots before CLR.");

                        WinFormsTestHelper.FindByTextPrefix<Button>(form, "CLR").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        Assert.AreEqual(string.Empty, GetDotsLabel(form).Text,
                            "CLR button should clear all entered digits.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        [TestMethod]
        public void PinLoginForm_BackButton_RemovesLastDigit()
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

                        form.OnKeyDown(form, new KeyEventArgs(Keys.D1));
                        form.OnKeyDown(form, new KeyEventArgs(Keys.D2));
                        Assert.AreEqual(2, GetDotsLabel(form).Text.Length, "Should have 2 dots before Back.");

                        WinFormsTestHelper.FindByTextPrefix<Button>(form, "Back").PerformClick();
                        WinFormsTestHelper.PumpEvents();

                        Assert.AreEqual(1, GetDotsLabel(form).Text.Length,
                            "Back button should remove exactly one digit.");
                    }
                }
                finally { DeleteTempDataDirectory(tempDir); }
            });
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static void TriggerLockout(PinLoginForm form)
        {
            for (int attempt = 0; attempt < 3; attempt++)
            {
                for (int i = 0; i < 4; i++)
                    form.OnKeyDown(form, new KeyEventArgs(Keys.D9));
                form.OnKeyDown(form, new KeyEventArgs(Keys.Enter));
                WinFormsTestHelper.PumpEvents();
            }
        }
    }
}
