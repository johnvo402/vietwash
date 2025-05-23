using AutoMapper;
using Domain.Aggregates.Funds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.FundBehaviors.Queries
{
    public class ListFundBehaviorMapping : Profile
    {
        public ListFundBehaviorMapping()
        {
            CreateMap<FundBehavior, ListFundBehaviorResponse>();
        }
    }
}
