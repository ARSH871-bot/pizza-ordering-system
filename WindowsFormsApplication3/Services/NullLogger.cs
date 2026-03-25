namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// No-op <see cref="ILogger"/> implementation.
    /// Use in unit tests and anywhere logging output is not needed.
    /// </summary>
    public sealed class NullLogger : ILogger
    {
        /// <summary>Shared singleton instance — safe to reuse across tests.</summary>
        public static readonly NullLogger Instance = new NullLogger();

        /// <inheritdoc/>
        public void Info(string message) { }

        /// <inheritdoc/>
        public void Warn(string message) { }

        /// <inheritdoc/>
        public void Error(string message, System.Exception ex = null) { }
    }
}
