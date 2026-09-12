using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mediator;
using Shared.Kernel.Common.Events;

namespace Domain.Aggregates.Orders.Events
{
    public class EInvoiceEvent : IDomainEvent
    {
        public Order Order { get; set; } = default!;
    }
}
