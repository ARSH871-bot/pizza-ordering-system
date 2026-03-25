using System.Collections.Generic;
using System.Text.RegularExpressions;
using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Pure validation logic — no UI dependency, fully unit-testable.
    /// </summary>
    public class OrderValidator : IOrderValidator
    {
        // ── Field-level validators ────────────────────────────────────────────

        public ValidationResult ValidatePostalCode(string postalCode)
        {
            if (!Regex.IsMatch(postalCode?.Trim() ?? string.Empty, @"^\d{4}$"))
                return ValidationResult.Fail(
                    "Please enter a valid New Zealand postal code (4 digits, e.g. 1010)");
            return ValidationResult.Ok();
        }

        public ValidationResult ValidateContactNo(string contactNo)
        {
            if (string.IsNullOrWhiteSpace(contactNo))
                return ValidationResult.Ok();   // optional field

            if (!Regex.IsMatch(contactNo.Trim(), @"^\+?\d{7,15}$"))
                return ValidationResult.Fail(
                    "Please enter a valid contact number (digits only, 7–15 digits)");
            return ValidationResult.Ok();
        }

        public ValidationResult ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return ValidationResult.Ok();   // optional field

            // Basic RFC-5322-inspired check: local@domain.tld
            if (!Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return ValidationResult.Fail("Please enter a valid email address (e.g. name@example.com).");

            return ValidationResult.Ok();
        }

        // ── Aggregate validators ─────────────────────────────────────────────

        public ValidationResult ValidateCustomer(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer?.FirstName))
                return ValidationResult.Fail("First Name is required.");
            if (string.IsNullOrWhiteSpace(customer.LastName))
                return ValidationResult.Fail("Last Name is required.");
            if (string.IsNullOrWhiteSpace(customer.Address))
                return ValidationResult.Fail("Address is required.");
            if (string.IsNullOrWhiteSpace(customer.PostalCode))
                return ValidationResult.Fail("Postal Code is required.");

            var postalResult = ValidatePostalCode(customer.PostalCode);
            if (!postalResult.IsValid) return postalResult;

            var contactResult = ValidateContactNo(customer.ContactNo);
            if (!contactResult.IsValid) return contactResult;

            return ValidateEmail(customer.Email);
        }

        public ValidationResult ValidatePayment(string paymentMethod, decimal amountPaid, decimal totalDue)
        {
            if (string.IsNullOrWhiteSpace(paymentMethod))
                return ValidationResult.Fail("Please select a payment method.");

            if (amountPaid < totalDue)
                return ValidationResult.Fail("Amount paid is less than the total due. Please pay your balance.");

            return ValidationResult.Ok();
        }

        public ValidationResult ValidateOrder(List<OrderItem> items)
        {
            if (items == null || items.Count == 0)
                return ValidationResult.Fail("Please select at least one item before proceeding.");

            bool hasPizza = items.Exists(i => i.Name != null && i.Name.EndsWith("Pizza"));
            if (!hasPizza)
                return ValidationResult.Fail("Please configure at least one pizza before proceeding.");

            return ValidationResult.Ok();
        }
    }
}
