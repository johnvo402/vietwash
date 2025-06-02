using Mediator;
using Contracts.Routers;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Common.Projections.Branches.Branch;


namespace Application.Features.Branches.Branch.Commands.Update
{
    public class UpdateBranchCommand : IRequest<UpdateBranchResponse>
    {
        [FromRoute( Name = RouterBase.Id)]
        public long BranchId { get; set; }
        [FromBody]
        public BranchModel? Branch { get; set; }
       
    }
}
