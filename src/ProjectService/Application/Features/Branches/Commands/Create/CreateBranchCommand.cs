using Application.Features.Common.Projections.Branches.Branch;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Branches.Commands.Create
{
    public class CreateBranchCommand : BranchModel, IRequest<Result>;
}
