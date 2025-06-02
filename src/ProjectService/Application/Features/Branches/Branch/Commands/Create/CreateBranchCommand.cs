using Application.Features.Common.Projections.Branches.Branch;
using Mediator;

namespace Application.Features.Branches.Branch.Commands.Create
{
    public class CreateBranchCommand : BranchModel, IRequest<CreateBranchResponse>
    {
    }
}
