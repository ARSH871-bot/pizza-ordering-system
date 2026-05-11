using WindowsFormsApplication3.Models;

namespace WindowsFormsApplication3.Services
{
    /// <summary>
    /// Coordinates receipt generation and durable persistence for a completed order.
    /// </summary>
    public interface IOrderSubmissionService
    {
        SubmittedOrder Submit(Order order);
    }

    public sealed class SubmittedOrder
    {
        public OrderRecord Record { get; set; }
        public string ReceiptText { get; set; }
    }
}
