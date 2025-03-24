using Domain.Aggregates.Orders.Enums;
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
        public decimal Amount { get; set; } = default!;
        public string Note { get; set; } = default!;
        public DateTimeOffset TransactionDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = default!;
        public FundBehavior FundBehavior { get; set; } = default!;
        public FundType FundType { get; set; } = default!;
        public Ulid? ReferenceId { get; set; }
		protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
