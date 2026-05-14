using System;
using System.Collections.Generic;
using System.Globalization;
using WindowsFormsApplication3.Config;
using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Services
{
    public sealed class CheckoutWorkflowService : ICheckoutWorkflowService
    {
        private readonly IPromoEngine    _promoEngine;
        private readonly IOrderValidator _validator;

        private static readonly CultureInfo CurrencyCulture = new CultureInfo("en-NZ");

        public CheckoutWorkflowService(IPromoEngine promoEngine, IOrderValidator validator)
        {
            _promoEngine = promoEngine ?? throw new ArgumentNullException("promoEngine");
            _validator   = validator   ?? throw new ArgumentNullException("validator");
        }

        public Customer BuildCustomer(string firstName, string lastName, string address,
            string city, string region, string postalCode, string contactNo, string email)
        {
            return new Customer
            {
                FirstName  = firstName,
                LastName   = lastName,
                Address    = address,
                City       = city,
                Region     = region,
                PostalCode = postalCode,
                ContactNo  = contactNo,
                Email      = email,
            };
        }

        public ValidationResult ValidateCustomer(Customer customer)
            => _validator.ValidateCustomer(customer);

        public PromoPaymentResult ApplyPromo(string promoCode, decimal originalTotal)
        {
            var result = _promoEngine.Apply(promoCode, originalTotal);
            return result.Success
                ? PromoPaymentResult.Succeeded(promoCode, result.DiscountedTotal, result.Message)
                : PromoPaymentResult.Failed(result.Message);
        }

        public StandardPaymentResult ProcessStandardPayment(string paymentMethod,
            decimal amountPaid, decimal totalDue)
        {
            var result = _validator.ValidatePayment(paymentMethod, amountPaid, totalDue);
            return result.IsValid
                ? StandardPaymentResult.Succeeded(amountPaid - totalDue)
                : StandardPaymentResult.Failed(result.ErrorMessage);
        }

        public Order AssembleOrder(Customer customer, string paymentMethod, string cardOrPromoText,
            string appliedPromoCode, decimal originalTotal, decimal discountedTotal,
            decimal amountPaid, int deliveryMinutes, IEnumerable<OrderItem> items)
        {
            var order = new Order
            {
                Customer            = customer,
                PaymentMethod       = paymentMethod,
                PaymentReference    = PaymentReferenceHelper.NormalizeForStorage(paymentMethod, cardOrPromoText),
                DiscountDescription = string.IsNullOrWhiteSpace(appliedPromoCode) ? null : appliedPromoCode.Trim(),
                DeliveryMinutes     = deliveryMinutes,
                AmountPaid          = amountPaid,
                Discount            = Math.Max(originalTotal - discountedTotal, 0m),
            };
            foreach (var item in items)
                order.Items.Add(item);
            return order;
        }

        public OrderRecord BuildOrderRecord(Order order)
        {
            var record = new OrderRecord
            {
                Id                  = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant(),
                OrderDate           = order.OrderDate,
                CustomerName        = order.Customer.FullName,
                Address             = order.Customer.Address,
                City                = order.Customer.City,
                Region              = order.Customer.Region,
                PostalCode          = order.Customer.PostalCode,
                PaymentMethod       = order.PaymentMethod,
                PaymentReference    = order.PaymentReference,
                Subtotal            = order.Subtotal,
                Tax                 = order.Tax,
                Total               = order.AmountDue,
                Discount            = order.Discount,
                DiscountDescription = order.DiscountDescription,
                Status              = "Active",
            };
            foreach (var item in order.Items)
                record.Lines.Add(new OrderLineRecord
                {
                    Item     = item.Name,
                    Quantity = item.Quantity,
                    Price    = item.TotalPrice,
                });
            return record;
        }

        public int GetDeliveryMinutes(ISettingsRepository settings)
        {
            string raw = settings?.Get("DeliveryMinutes", AppConfig.DeliveryMinutes.ToString());
            int minutes;
            return int.TryParse(raw, out minutes) && minutes > 0
                ? minutes
                : AppConfig.DeliveryMinutes;
        }

        internal static decimal ParseCurrencyOrZero(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0m;
            decimal value;
            if (decimal.TryParse(text, NumberStyles.Currency, CurrencyCulture, out value))
                return value;
            if (decimal.TryParse(text, NumberStyles.Currency, CultureInfo.CurrentCulture, out value))
                return value;
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
                return value;
            return 0m;
        }
    }
}
