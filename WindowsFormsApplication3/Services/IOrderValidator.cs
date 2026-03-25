using System.Collections.Generic;
using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Validates order data — postal codes, contact numbers, customer details,
    /// payment amounts, and order contents.
    /// </summary>
    public interface IOrderValidator
    {
        /// <summary>Validates a New Zealand 4-digit postal code.</summary>
        ValidationResult ValidatePostalCode(string postalCode);

        /// <summary>Validates an optional phone number (digits only, 7–15 characters).</summary>
        ValidationResult ValidateContactNo(string contactNo);

        /// <summary>Validates all required customer delivery fields.</summary>
        ValidationResult ValidateCustomer(Customer customer);

        /// <summary>Validates that a payment method is selected and the amount paid covers the total.</summary>
        ValidationResult ValidatePayment(string paymentMethod, decimal amountPaid, decimal totalDue);

        /// <summary>Validates that the order contains at least one pizza item.</summary>
        ValidationResult ValidateOrder(List<OrderItem> items);
    }
}
