using System;
using System.IO;
using System.Windows.Forms;
using WindowsFormsApplication3.Forms;
using WindowsFormsApplication3.Infrastructure;
using WindowsFormsApplication3.Services;

namespace WindowsFormsApplication3
{
    /// <summary>Application entry point and composition root.</summary>
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // ── Global exception handlers ──────────────────────────────────────
            Application.ThreadException += OnUnhandledUiException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledDomainException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ── Composition root ───────────────────────────────────────────────
            // All services are wired here — Form1 receives them via constructor.
            // This is the only place in the codebase that calls 'new' on concrete
            // service implementations; everywhere else depends on interfaces.
            string dataDir = DefaultDataDirectory();

            // 1. Run database migrations (idempotent — safe on every startup).
            DatabaseMigrator.Run(dataDir);

            // 2. Wire services.
            IOrderRepository   repo     = new OrderRepository(dataDir);
            ISettingsRepository settings = new SettingsRepository(dataDir);
            ICartService        cart     = new CartService(settings);

            // 3. Staff PIN check (bypassed when no PIN is configured).
            if (PinLoginForm.PinRequired(settings))
            {
                using (var login = new PinLoginForm(settings))
                {
                    if (login.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                        return;   // user cancelled — exit cleanly
                }
            }

            Application.Run(new Form1(repo, cart, settings));
        }

        internal static string DefaultDataDirectory()
            => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PizzaExpress");

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
