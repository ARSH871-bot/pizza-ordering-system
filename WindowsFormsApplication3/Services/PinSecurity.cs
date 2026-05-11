using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Protects staff PINs using PBKDF2 and validates operator-entered PIN values.
    /// </summary>
    public static class PinSecurity
    {
        private const string Prefix = "PBKDF2";
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static bool IsConfigured(string storedPin)
            => !string.IsNullOrWhiteSpace(storedPin);

        public static bool IsProtected(string storedPin)
            => !string.IsNullOrWhiteSpace(storedPin) &&
               storedPin.StartsWith(Prefix + "$", StringComparison.Ordinal);

        public static string Protect(string plainTextPin)
        {
            ValidationResult validation = ValidateNewPin(plainTextPin);
            if (!validation.IsValid)
                throw new ArgumentException(validation.ErrorMessage, "plainTextPin");

            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            byte[] hash = Hash(plainTextPin.Trim(), salt, Iterations);
            return string.Format(
                "{0}${1}${2}${3}",
                Prefix,
                Iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public static bool Verify(string enteredPin, string storedPin)
        {
            string candidate = (enteredPin ?? string.Empty).Trim();
            string stored = (storedPin ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(stored))
                return false;

            if (!IsProtected(stored))
                return ConstantTimeEquals(
                    Encoding.UTF8.GetBytes(candidate),
                    Encoding.UTF8.GetBytes(stored));

            string[] parts = stored.Split('$');
            if (parts.Length != 4 || parts[0] != Prefix)
                return false;

            int iterations;
            if (!int.TryParse(parts[1], out iterations) || iterations <= 0)
                return false;

            byte[] salt;
            byte[] expectedHash;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expectedHash = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] actualHash = Hash(candidate, salt, iterations);
            return ConstantTimeEquals(actualHash, expectedHash);
        }

        public static ValidationResult ValidateNewPin(string plainTextPin)
        {
            string candidate = (plainTextPin ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(candidate))
                return ValidationResult.Ok();

            if (!Regex.IsMatch(candidate, @"^\d{4,12}$"))
            {
                return ValidationResult.Fail(
                    "Staff PIN must be 4 to 12 digits. Leave it blank to disable the PIN.");
            }

            return ValidationResult.Ok();
        }

        private static byte[] Hash(string value, byte[] salt, int iterations)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(value, salt, iterations))
                return deriveBytes.GetBytes(HashSize);
        }

        private static bool ConstantTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < left.Length; i++)
                diff |= left[i] ^ right[i];

            return diff == 0;
        }
    }
}
