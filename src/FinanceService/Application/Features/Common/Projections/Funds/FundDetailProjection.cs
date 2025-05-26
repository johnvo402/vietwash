using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Users;

namespace Application.Features.Common.Projections.Funds
{
    public class FundDetailProjection : UpdateFundModel
    {
        public User User { get; set; } = default!;
    }
}
