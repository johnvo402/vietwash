using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Projections.Branches.Branch;
using AutoMapper;
using Domain.Aggregates.Branches;

namespace Application.Features.Branches.Branch.Commands.Create
{
    public class CreateBranchMapping : Profile
    {
        public CreateBranchMapping()
        {
            CreateMap<BranchModel, Domain.Aggregates.Branches.Branch>();
        }
    }
}
