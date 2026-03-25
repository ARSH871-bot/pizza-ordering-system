namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Minimal structured logger interface.
    /// Services receive this via constructor injection.
    /// </summary>
    public interface ILogger
    {
        /// <summary>Records an informational event (normal operation).</summary>
        void Info(string message);

        /// <summary>Records a warning (unexpected but recoverable situation).</summary>
        void Warn(string message);

        /// <summary>Records an error with optional exception details.</summary>
        void Error(string message, System.Exception ex = null);
    }
}
