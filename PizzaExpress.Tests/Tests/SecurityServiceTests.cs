using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    [TestClass]
    public class SecurityServiceTests
    {
        // ── PinSecurity.IsConfigured ──────────────────────────────────────────

        [TestMethod]
        public void IsConfigured_Null_ReturnsFalse()
            => Assert.IsFalse(PinSecurity.IsConfigured(null));

        [TestMethod]
        public void IsConfigured_Empty_ReturnsFalse()
            => Assert.IsFalse(PinSecurity.IsConfigured(""));

        [TestMethod]
        public void IsConfigured_Whitespace_ReturnsFalse()
            => Assert.IsFalse(PinSecurity.IsConfigured("   "));

        [TestMethod]
        public void IsConfigured_NonEmptyValue_ReturnsTrue()
            => Assert.IsTrue(PinSecurity.IsConfigured("anyvalue"));

        // ── PinSecurity.IsProtected ───────────────────────────────────────────

        [TestMethod]
        public void IsProtected_Null_ReturnsFalse()
            => Assert.IsFalse(PinSecurity.IsProtected(null));

        [TestMethod]
        public void IsProtected_PlaintextPin_ReturnsFalse()
            => Assert.IsFalse(PinSecurity.IsProtected("1234"));

        [TestMethod]
        public void IsProtected_ProtectedHash_ReturnsTrue()
        {
            string hash = PinSecurity.Protect("1234");
            Assert.IsTrue(PinSecurity.IsProtected(hash));
        }

        // ── PinSecurity.ValidateNewPin ────────────────────────────────────────

        [TestMethod]
        public void ValidateNewPin_Blank_IsValid()
            => Assert.IsTrue(PinSecurity.ValidateNewPin("").IsValid);

        [TestMethod]
        public void ValidateNewPin_Null_IsValid()
            => Assert.IsTrue(PinSecurity.ValidateNewPin(null).IsValid);

        [TestMethod]
        public void ValidateNewPin_4Digits_IsValid()
            => Assert.IsTrue(PinSecurity.ValidateNewPin("1234").IsValid);

        [TestMethod]
        public void ValidateNewPin_12Digits_IsValid()
            => Assert.IsTrue(PinSecurity.ValidateNewPin("123456789012").IsValid);

        [TestMethod]
        public void ValidateNewPin_TooShort_IsInvalid()
            => Assert.IsFalse(PinSecurity.ValidateNewPin("123").IsValid);

        [TestMethod]
        public void ValidateNewPin_TooLong_IsInvalid()
            => Assert.IsFalse(PinSecurity.ValidateNewPin("1234567890123").IsValid);

        [TestMethod]
        public void ValidateNewPin_NonDigits_IsInvalid()
            => Assert.IsFalse(PinSecurity.ValidateNewPin("abcd").IsValid);

        // ── PinSecurity.Verify ────────────────────────────────────────────────

        [TestMethod]
        public void Verify_CorrectPin_ReturnsTrue()
        {
            string hash = PinSecurity.Protect("9876");
            Assert.IsTrue(PinSecurity.Verify("9876", hash));
        }

        [TestMethod]
        public void Verify_WrongPin_ReturnsFalse()
        {
            string hash = PinSecurity.Protect("9876");
            Assert.IsFalse(PinSecurity.Verify("1111", hash));
        }

        [TestMethod]
        public void Verify_EmptyStoredPin_ReturnsFalse()
            => Assert.IsFalse(PinSecurity.Verify("1234", ""));

        [TestMethod]
        public void Verify_NullStoredPin_ReturnsFalse()
            => Assert.IsFalse(PinSecurity.Verify("1234", null));

        [TestMethod]
        public void Verify_LegacyPlaintextPin_MatchesExact()
            => Assert.IsTrue(PinSecurity.Verify("1234", "1234"));

        [TestMethod]
        public void Verify_LegacyPlaintextPin_WrongValue_ReturnsFalse()
            => Assert.IsFalse(PinSecurity.Verify("9999", "1234"));

        [TestMethod]
        public void Verify_MalformedHash_TooFewParts_ReturnsFalse()
            => Assert.IsFalse(PinSecurity.Verify("1234", "PBKDF2$100000$invalidsalt"));

        [TestMethod]
        public void Verify_MalformedHash_BadBase64_ReturnsFalse()
            => Assert.IsFalse(PinSecurity.Verify("1234", "PBKDF2$100000$!!!$!!!"));

        [TestMethod]
        public void Verify_MalformedHash_NonIntegerIterations_ReturnsFalse()
            => Assert.IsFalse(PinSecurity.Verify("1234", "PBKDF2$notanumber$c2FsdA==$aGFzaA=="));

        [TestMethod]
        public void Verify_PinWithSurroundingSpaces_StillVerifies()
        {
            string hash = PinSecurity.Protect("1234");
            Assert.IsTrue(PinSecurity.Verify("  1234  ", hash));
        }

        // ── StaffAuthSession ──────────────────────────────────────────────────

        [TestInitialize]
        public void ResetAuthSession()
        {
            // Reset static state before each test in this class
            typeof(StaffAuthSession)
                .GetField("_lastAuthenticatedUtc", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, DateTime.MinValue);
        }

        [TestMethod]
        public void HasRecentAuthorization_NeverAuthenticated_ReturnsFalse()
            => Assert.IsFalse(StaffAuthSession.HasRecentAuthorization(TimeSpan.FromMinutes(10)));

        [TestMethod]
        public void HasRecentAuthorization_JustAuthenticated_ReturnsTrue()
        {
            StaffAuthSession.MarkAuthenticated();
            Assert.IsTrue(StaffAuthSession.HasRecentAuthorization(TimeSpan.FromMinutes(10)));
        }

        [TestMethod]
        public void HasRecentAuthorization_SessionExpired_ReturnsFalse()
        {
            // Force the last-auth time to be 5 seconds in the past; use a maxAge of 1 second
            typeof(StaffAuthSession)
                .GetField("_lastAuthenticatedUtc", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, DateTime.UtcNow - TimeSpan.FromSeconds(5));

            Assert.IsFalse(StaffAuthSession.HasRecentAuthorization(TimeSpan.FromSeconds(1)));
        }
    }
}
