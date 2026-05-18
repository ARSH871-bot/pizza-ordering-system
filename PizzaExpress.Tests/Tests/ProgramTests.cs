using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3;

namespace PizzaExpress.Tests.Tests
{
    [DoNotParallelize]
    [TestClass]
    public class ProgramTests
    {
        // ── CrashLogPath ──────────────────────────────────────────────────────

        [TestMethod]
        public void CrashLogPath_ContainsPizzaExpressAndLogs()
        {
            var mi = typeof(Program).GetMethod("CrashLogPath",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(mi, "CrashLogPath not found via reflection.");
            string path = (string)mi.Invoke(null, null);
            StringAssert.Contains(path, "PizzaExpress");
            StringAssert.Contains(path, "Logs");
        }

        // ── WriteCrashLog ─────────────────────────────────────────────────────

        [TestMethod]
        public void WriteCrashLog_WithException_CreatesLogFile()
        {
            var mi = typeof(Program).GetMethod("WriteCrashLog",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(mi, "WriteCrashLog not found via reflection.");

            string logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PizzaExpress", "Logs");

            var before = Directory.Exists(logDir)
                ? Directory.GetFiles(logDir, "crash_*.log").Length
                : 0;

            mi.Invoke(null, new object[] { new Exception("unit-test crash") });

            int after = Directory.Exists(logDir)
                ? Directory.GetFiles(logDir, "crash_*.log").Length
                : 0;

            Assert.IsTrue(after > before, "WriteCrashLog should have created at least one crash log file.");
        }

        // ── OnUnhandledDomainException ────────────────────────────────────────

        [TestMethod]
        public void OnUnhandledDomainException_WithException_DoesNotThrow()
        {
            var mi = typeof(Program).GetMethod("OnUnhandledDomainException",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(mi, "OnUnhandledDomainException not found via reflection.");

            var args = new UnhandledExceptionEventArgs(new Exception("domain ex"), false);
            mi.Invoke(null, new object[] { null, args });
        }

        [TestMethod]
        public void OnUnhandledDomainException_WithNonException_DoesNotThrow()
        {
            var mi = typeof(Program).GetMethod("OnUnhandledDomainException",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(mi, "OnUnhandledDomainException not found via reflection.");

            var args = new UnhandledExceptionEventArgs("not an exception object", false);
            mi.Invoke(null, new object[] { null, args });
        }

        // ── OnUnhandledUiException ────────────────────────────────────────────

        [TestMethod]
        public void OnUnhandledUiException_ShowsErrorMessageBoxAndLogsFile()
        {
            WinFormsTestHelper.RunInSta(() =>
            {
                var mi = typeof(Program).GetMethod("OnUnhandledUiException",
                    BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(mi, "OnUnhandledUiException not found via reflection.");

                var args = new ThreadExceptionEventArgs(new Exception("test ui exception"));
                using (new WinFormsTestHelper.DialogAutoCloser("Unexpected Error"))
                    mi.Invoke(null, new object[] { null, args });
            });
        }
    }
}
