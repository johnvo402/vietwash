using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mediator;

namespace Domain.Aggregates.Orders.Events
{
    public class EInvoiceEvent : INotification
    {
        public Order Order { get; set; } = default!;
    }
}
