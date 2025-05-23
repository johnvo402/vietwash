using Domain.Aggregates.Funds.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Features.Common.Projections.Funds
{
    public class FundProjection
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public long? TypeId { get; set; }
        public long? BehaviorId { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset TransactionDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentMethod PaymentMethod { get; set; }
        public long ReferenceId { get; set; }
    }
}
