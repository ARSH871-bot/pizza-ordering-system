using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    [TestClass]
    public class PaymentReferenceHelperTests
    {
        // ── RequiresReference ─────────────────────────────────────────────────

        [TestMethod]
        public void RequiresReference_Cash_ReturnsFalse()
            => Assert.IsFalse(PaymentReferenceHelper.RequiresReference("Cash"));

        [TestMethod]
        public void RequiresReference_PromoCard_ReturnsFalse()
            => Assert.IsFalse(PaymentReferenceHelper.RequiresReference("Promo Card"));

        [TestMethod]
        public void RequiresReference_EmptyString_ReturnsFalse()
            => Assert.IsFalse(PaymentReferenceHelper.RequiresReference(""));

        [TestMethod]
        public void RequiresReference_Null_ReturnsFalse()
            => Assert.IsFalse(PaymentReferenceHelper.RequiresReference(null));

        [TestMethod]
        public void RequiresReference_CreditCard_ReturnsTrue()
            => Assert.IsTrue(PaymentReferenceHelper.RequiresReference("Credit Card"));

        [TestMethod]
        public void RequiresReference_DebitCard_ReturnsTrue()
            => Assert.IsTrue(PaymentReferenceHelper.RequiresReference("Debit Card"));

        // ── NormalizeForStorage ───────────────────────────────────────────────

        [TestMethod]
        public void NormalizeForStorage_Cash_ReturnsNull()
            => Assert.IsNull(PaymentReferenceHelper.NormalizeForStorage("Cash", "anything"));

        [TestMethod]
        public void NormalizeForStorage_NullReference_ReturnsNull()
            => Assert.IsNull(PaymentReferenceHelper.NormalizeForStorage("Credit Card", null));

        [TestMethod]
        public void NormalizeForStorage_WhitespaceReference_ReturnsNull()
            => Assert.IsNull(PaymentReferenceHelper.NormalizeForStorage("Credit Card", "   "));

        [TestMethod]
        public void NormalizeForStorage_CardWithAtLeast12Digits_MasksToLast4()
        {
            string result = PaymentReferenceHelper.NormalizeForStorage("Credit Card", "4111111111111111");
            Assert.AreEqual("****1111", result);
        }

        [TestMethod]
        public void NormalizeForStorage_ShortAlphanumericReference_ReturnsAsIs()
        {
            string result = PaymentReferenceHelper.NormalizeForStorage("Credit Card", "REF-001");
            Assert.AreEqual("REF-001", result);
        }

        [TestMethod]
        public void NormalizeForStorage_MultipleInternalSpaces_CollapsedToSingle()
        {
            string result = PaymentReferenceHelper.NormalizeForStorage("Credit Card", "REF  001");
            Assert.AreEqual("REF 001", result);
        }

        [TestMethod]
        public void NormalizeForStorage_ReferenceExceeds30Chars_TruncatesTo30()
        {
            string longRef = "A" + new string('B', 35);
            string result  = PaymentReferenceHelper.NormalizeForStorage("Debit Card", longRef);
            Assert.AreEqual(30, result.Length);
        }

        [TestMethod]
        public void NormalizeForStorage_PromoCard_ReturnsNull()
            => Assert.IsNull(PaymentReferenceHelper.NormalizeForStorage("Promo Card", "PIZZA10"));
    }
}
