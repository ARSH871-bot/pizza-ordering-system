using System;
using System.IO;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Appends timestamped log entries to a daily rotating file in
    /// <c>%APPDATA%\PizzaExpress\Logs\app_yyyy-MM-dd.log</c>.
    /// Thread-safe via a per-write lock.
    /// </summary>
    public class FileLogger : ILogger
    {
        private readonly string _logDir;
        private readonly object _lock = new object();

        /// <summary>Uses the default log directory under %APPDATA%\PizzaExpress\Logs.</summary>
        public FileLogger()
            : this(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PizzaExpress", "Logs"))
        {
        }

        /// <summary>Uses a custom log directory — primarily for tests.</summary>
        public FileLogger(string logDirectory)
        {
            _logDir = logDirectory;
        }

        /// <inheritdoc/>
        public void Info(string message)  => Write("INFO ", message, null);

        /// <inheritdoc/>
        public void Warn(string message)  => Write("WARN ", message, null);

        /// <inheritdoc/>
        public void Error(string message, Exception ex = null) => Write("ERROR", message, ex);

        private void Write(string level, string message, Exception ex)
        {
            try
            {
                lock (_lock)
                {
                    Directory.CreateDirectory(_logDir);
                    string path  = Path.Combine(_logDir, $"app_{DateTime.Today:yyyy-MM-dd}.log");
                    string entry = $"{DateTime.Now:HH:mm:ss.fff}  [{level}]  {message}";

                    if (ex != null)
                        entry += $"\n            {ex.GetType().Name}: {ex.Message}";

                    File.AppendAllText(path, entry + Environment.NewLine);
                }
            }
            catch
            {
                // Logging must never throw — swallow all I/O errors silently.
            }
        }
    }
}
