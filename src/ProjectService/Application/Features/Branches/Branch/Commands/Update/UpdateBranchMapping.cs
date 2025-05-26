using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Projections.Branches.Branch;
using AutoMapper;
using static Application.Features.Branches.Branch.Commands.Update.UpdateBranchCommand;

namespace Application.Features.Branches.Branch.Commands.Update
{
    public class UpdateBranchMapping : Profile
    {
        public UpdateBranchMapping()
        {
            CreateMap<UpdateBranch, Domain.Aggregates.Branches.Branch>();
            CreateMap<Domain.Aggregates.Branches.Branch, UpdateBranchResponse>().IncludeBase<Domain.Aggregates.Branches.Branch, BranchDetailProjection>();
        }
    }
}
