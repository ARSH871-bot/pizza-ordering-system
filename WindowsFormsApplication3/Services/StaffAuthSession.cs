using System;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Tracks recent staff authentication so the app can re-prompt for sensitive actions
    /// without forcing repeated PIN entry for every click.
    /// </summary>
    public static class StaffAuthSession
    {
        private static DateTime _lastAuthenticatedUtc = DateTime.MinValue;

        public static void MarkAuthenticated()
        {
            _lastAuthenticatedUtc = DateTime.UtcNow;
        }

        public static bool HasRecentAuthorization(TimeSpan maxAge)
        {
            if (_lastAuthenticatedUtc == DateTime.MinValue)
                return false;

            return DateTime.UtcNow - _lastAuthenticatedUtc <= maxAge;
        }
    }
}
