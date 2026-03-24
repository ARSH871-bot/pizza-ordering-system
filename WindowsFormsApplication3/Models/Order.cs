using System;
using System.Collections.Generic;
using System.Linq;
using WindowsFormsApplication3.Config;

namespace WindowsFormsApplication3.Models
{
    public class Order
    {
        public List<OrderItem> Items       { get; }      = new List<OrderItem>();
        public Customer        Customer    { get; set; } = new Customer();
        public string          PaymentMethod { get; set; }
        public decimal         AmountPaid  { get; set; }
        public DateTime        OrderDate   { get; set; } = DateTime.Now;

        public decimal Subtotal => Items.Sum(i => i.TotalPrice);
        public decimal Tax      => Math.Round(Subtotal * AppConfig.TaxRate, 2);
        public decimal Total    => Subtotal + Tax;
        public decimal Change   => AmountPaid - Total;
    }
}
