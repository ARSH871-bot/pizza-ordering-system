using System.Collections.Generic;
using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Services
{
    public interface ICheckoutWorkflowService
    {
        Customer BuildCustomer(string firstName, string lastName, string address,
            string city, string region, string postalCode, string contactNo, string email);

        ValidationResult ValidateCustomer(Customer customer);

        PromoPaymentResult ApplyPromo(string promoCode, decimal originalTotal);

        StandardPaymentResult ProcessStandardPayment(string paymentMethod,
            decimal amountPaid, decimal totalDue);

        Order AssembleOrder(Customer customer, string paymentMethod, string cardOrPromoText,
            string appliedPromoCode, decimal originalTotal, decimal discountedTotal,
            decimal amountPaid, int deliveryMinutes, IEnumerable<OrderItem> items);

        OrderRecord BuildOrderRecord(Order order);

        int GetDeliveryMinutes(ISettingsRepository settings);
    }

    public sealed class PromoPaymentResult
    {
        public bool    Success        { get; private set; }
        public string  Message        { get; private set; }
        public decimal DiscountedTotal { get; private set; }
        public string  AppliedCode    { get; private set; }

        public static PromoPaymentResult Succeeded(string code, decimal discountedTotal, string message)
            => new PromoPaymentResult { Success = true, AppliedCode = code,
                                        DiscountedTotal = discountedTotal, Message = message };

        public static PromoPaymentResult Failed(string message)
            => new PromoPaymentResult { Success = false, Message = message };
    }

    public sealed class StandardPaymentResult
    {
        public bool    Success      { get; private set; }
        public string  ErrorMessage { get; private set; }
        public decimal Change       { get; private set; }

        public static StandardPaymentResult Succeeded(decimal change)
            => new StandardPaymentResult { Success = true, Change = change };

        public static StandardPaymentResult Failed(string errorMessage)
            => new StandardPaymentResult { Success = false, ErrorMessage = errorMessage };
    }
}
