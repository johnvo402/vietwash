using Domain.Aggregates.Funds.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Common.Projections.Funds
{
    public class UpdateFundModel
    {
        public string? Name { get; set; }
        public string? FundTpye { get; set; }
        public decimal? Amount { get; set; }
        public long BehaviorId { get; set; }
        public string? Note { get; set; }
        public FundStatus Status { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; } = default!;
        public DateTimeOffset TransactionDate { get; set; } = default!;
        public long BranchId { get; set; } = default!;
    }
}
