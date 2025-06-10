using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Projections.Branches.Branch;
using AutoMapper;

namespace Application.Features.Branches.Branch.Queries
{
    public class ListBranchMapping : Profile
    {
        public ListBranchMapping()
        {
            CreateMap<Domain.Aggregates.Branches.Branch, BranchProjection>();
            CreateMap<Domain.Aggregates.Branches.Branch, ListBranchResponse>().IncludeBase<Domain.Aggregates.Branches.Branch, BranchProjection>();
        }
    }
}
