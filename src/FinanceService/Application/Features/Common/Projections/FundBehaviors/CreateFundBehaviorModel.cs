using Domain.Aggregates.Funds.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Common.Projections.FundBehaviors
{
    public class CreateFundBehaviorModel
    {
        public string Name { get; set; } = default!;

        public FundType Type { get; set; } = default!;
    }
}
