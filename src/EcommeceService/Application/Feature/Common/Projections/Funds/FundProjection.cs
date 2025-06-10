using Domain.Aggregates.Orders.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Funds
{
    public class FundProjection
    {
        public Ulid Id { get; set; }
        public string Name { get; set; }
        public string TypeId { get; set; }
        public string BehaviorId { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset TransactionDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentMethod PaymentMethod { get; set; }
        public Ulid ReferenceId { get; set; }
    }
}
