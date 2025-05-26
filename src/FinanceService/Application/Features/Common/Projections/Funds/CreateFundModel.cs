using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;
using Domain.Aggregates.Users;
using Mediator;

namespace Application.Features.Common.Projections.Funds
{
    public class CreateFundModel
    {
        public string Name { get; set; } = default!;
        public FundType Type { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public long FundBehaviorId { get; set; }
        public long ObjectId { get; set; }
        public string Note { get; set; } = default!;
        public FundStatus Status { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; } = default!;
        public long BranchId { get; set; } = default!;
    }
}
