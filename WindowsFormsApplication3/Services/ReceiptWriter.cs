using System.IO;
using System.Text;
using WindowsFormsApplication3.Config;
using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Builds and saves order receipts.
    /// Depends only on domain models — no UI or WinForms references.
    /// </summary>
    public class ReceiptWriter : IReceiptWriter
    {
        public string Build(Order order)
        {
            var sb = new StringBuilder();

            sb.AppendLine("========================================");
            sb.AppendLine("      PIZZA EXPRESS NEW ZEALAND");
            sb.AppendLine("========================================");
            sb.AppendLine($"Date: {order.OrderDate:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            sb.AppendLine("CUSTOMER INFORMATION");
            sb.AppendLine($"Name:        {order.Customer.FullName}");
            sb.AppendLine($"Address:     {order.Customer.Address}");
            sb.AppendLine($"City:        {order.Customer.City}");
            sb.AppendLine($"Region:      {order.Customer.Region}");
            sb.AppendLine($"Postal Code: {order.Customer.PostalCode}");
            sb.AppendLine($"Contact No:  {order.Customer.ContactNo}");
            sb.AppendLine();

            sb.AppendLine("ORDER SUMMARY");
            sb.AppendLine(string.Format("{0,-38} {1,5} {2,10}", "Item", "Qty", $"Price {AppConfig.CurrencyCode}"));
            sb.AppendLine(new string('-', 55));

            foreach (var item in order.Items)
            {
                sb.AppendLine(string.Format("{0,-38} {1,5} {2,10}",
                    item.Name,
                    item.Quantity > 0 ? item.Quantity.ToString() : string.Empty,
                    item.TotalPrice.ToString("F2")));
            }

            sb.AppendLine(new string('-', 55));
            sb.AppendLine(string.Format("{0,-44} {1,10}", "Subtotal:",                             order.Subtotal.ToString("C2")));
            sb.AppendLine(string.Format("{0,-44} {1,10}", $"{AppConfig.TaxLabel} ({AppConfig.TaxRate:P0}):", order.Tax.ToString("C2")));
            if (order.Discount > 0)
            {
                string label = string.IsNullOrWhiteSpace(order.DiscountDescription)
                    ? "Discount:"
                    : $"Discount ({order.DiscountDescription}):";
                sb.AppendLine(string.Format("{0,-44} {1,10}", label, ("-" + order.Discount.ToString("C2"))));
            }
            sb.AppendLine(string.Format("{0,-44} {1,10}", "Total Due:",                            order.AmountDue.ToString("C2")));
            sb.AppendLine();

            sb.AppendLine("PAYMENT");
            sb.AppendLine($"Method:      {order.PaymentMethod}");
            sb.AppendLine($"Amount Paid: {order.AmountPaid:C2}");
            sb.AppendLine($"Change:      {order.Change:C2}");
            sb.AppendLine();

            sb.AppendLine("========================================");
            sb.AppendLine(" Thank you for ordering at Pizza Express!");
            sb.AppendLine($" Delivery in approx. {order.DeliveryMinutes} minutes.");
            sb.AppendLine("========================================");

            return sb.ToString();
        }

        public void SaveToFile(Order order, string filePath)
        {
            File.WriteAllText(filePath, Build(order));
        }
    }
}
