using System;
using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Creates a persistent order record, saves it, and returns the generated receipt text.
    /// </summary>
    public sealed class OrderSubmissionService : IOrderSubmissionService
    {
        private readonly IOrderRepository _repo;
        private readonly IReceiptWriter _receiptWriter;

        public OrderSubmissionService(IOrderRepository repo, IReceiptWriter receiptWriter)
        {
            _repo = repo ?? throw new ArgumentNullException("repo");
            _receiptWriter = receiptWriter ?? throw new ArgumentNullException("receiptWriter");
        }

        public SubmittedOrder Submit(Order order)
        {
            if (order == null) throw new ArgumentNullException("order");

            var record = CreateRecord(order);
            _repo.Save(record);

            return new SubmittedOrder
            {
                Record = record,
                ReceiptText = _receiptWriter.Build(order),
            };
        }

        private static OrderRecord CreateRecord(Order order)
        {
            var record = new OrderRecord
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant(),
                OrderDate = order.OrderDate,
                CustomerName = order.Customer.FullName,
                Address = order.Customer.Address,
                City = order.Customer.City,
                Region = order.Customer.Region,
                PostalCode = order.Customer.PostalCode,
                PaymentMethod = order.PaymentMethod,
                Subtotal = order.Subtotal,
                Tax = order.Tax,
                Total = order.AmountDue,
                Discount = order.Discount,
                DiscountDescription = order.DiscountDescription,
                Status = "Active",
            };

            foreach (var item in order.Items)
            {
                record.Lines.Add(new OrderLineRecord
                {
                    Item = item.Name,
                    Quantity = item.Quantity,
                    Price = item.TotalPrice,
                });
            }

            return record;
        }
    }
}
