using Application.Features.Common.Projections.Branches.Branch;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Branches.Commands.Update
{
    public class UpdateBranchCommand : IRequest<Result>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long BranchId { get; set; }

        [FromBody]
        public BranchModel? Branch { get; set; }
    }
}
