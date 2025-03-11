using Elasticsearch.Net;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Funds
{
    public class Fund : AggregateRoot
    {
        public string Name { get; set; } = default!;
        public string TypeId { get; set; } = default!;
        public string BehaviorId { get; set; } = default!;
        public long Amount { get; set; } = default!;
        public string Note { get; set; } = default!;
        public string TransactionDate { get; set; } = default!;
        public string PaymentMethodId { get; set; } = default!;

       public PaymentMethod PaymentMethod { get; set; }=default!;
        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
