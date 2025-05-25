using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Projections.Branches.Branch;
using AutoMapper;
using Domain.Aggregates.Branches;

namespace Application.Features.Common.Mapping.Branches
{
    public class BranchMapping : Profile
    {
        public BranchMapping()
        {
            CreateMap<Branch, BranchProjection>();
            CreateMap<Branch, BranchDetailProjection>();
        }
    }
}
