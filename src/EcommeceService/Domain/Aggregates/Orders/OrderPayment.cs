using Domain.Aggregates.Funds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Orders
{
    public class OrderPayment
    {
        public Ulid OrderId { get; set; } = default!;
        public Ulid PaymentMethodId { get; set; } = default!;

        public long Amount { get; set; } = default!;

        public ICollection<Order> Orders { get; set; } = [];
        public ICollection<PaymentMethod> PaymentMethods { get; set; } = [];
    }
}
