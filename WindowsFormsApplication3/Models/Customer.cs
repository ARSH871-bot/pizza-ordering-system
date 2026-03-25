namespace WindowsFormsApplication3.Models
{
    /// <summary>
    /// Holds a customer's delivery contact information.
    /// All fields are optional at construction; <see cref="IOrderValidator.ValidateCustomer"/>
    /// enforces required fields before an order is confirmed.
    /// </summary>
    public class Customer
    {
        public string FirstName  { get; set; }
        public string LastName   { get; set; }
        public string Address    { get; set; }
        public string City       { get; set; }
        public string Region     { get; set; }
        public string PostalCode { get; set; }
        public string ContactNo  { get; set; }
        public string Email      { get; set; }

        public string FullName =>
            string.IsNullOrWhiteSpace(LastName)
                ? FirstName ?? string.Empty
                : $"{FirstName} {LastName}".Trim();
    }
}
