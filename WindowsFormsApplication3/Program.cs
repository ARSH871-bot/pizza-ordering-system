using System;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApplication3
{
    /// <summary>Application entry point.</summary>
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // ── Global exception handlers ──────────────────────────────────────
            // These catch unhandled exceptions on the UI thread and background
            // threads, write a crash log, and show a friendly message instead of
            // a raw .NET crash dialog.
            Application.ThreadException += OnUnhandledUiException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledDomainException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void OnUnhandledUiException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            WriteCrashLog(e.Exception);
            MessageBox.Show(
                $"An unexpected error occurred and has been logged.\n\n" +
                $"Error: {e.Exception.Message}\n\n" +
                $"Crash log saved to:\n{CrashLogPath()}",
                "Pizza Express — Unexpected Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private static void OnUnhandledDomainException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                WriteCrashLog(ex);
        }

        private static void WriteCrashLog(Exception ex)
        {
            try
            {
                string logDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PizzaExpress", "Logs");
                Directory.CreateDirectory(logDir);

                string path = Path.Combine(logDir, $"crash_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                File.WriteAllText(path,
                    $"Pizza Express Crash Log\n" +
                    $"========================\n" +
                    $"Date/Time : {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                    $"Version   : {Application.ProductVersion}\n\n" +
                    $"Exception : {ex.GetType().FullName}\n" +
                    $"Message   : {ex.Message}\n\n" +
                    $"Stack Trace:\n{ex.StackTrace}\n");
            }
            catch
            {
                // If we can't write a log, swallow silently — never throw inside an exception handler.
            }
        }

        /// <summary>Returns the crash log directory path for display in error messages.</summary>
        private static string CrashLogPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PizzaExpress", "Logs");
        }
    }
}
