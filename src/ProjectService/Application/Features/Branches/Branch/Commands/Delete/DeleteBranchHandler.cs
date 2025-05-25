
using Application.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Branches.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;

namespace Application.Features.Branches.Branch.Commands.Delete
{
    public class DeleteBranchHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteBranchCommand>
    {
        public async ValueTask<Unit> Handle(DeleteBranchCommand command, CancellationToken cancellationToken)
        {
            Domain.Aggregates.Branches.Branch branch = await unitOfWork.Repository<Domain.Aggregates.Branches.Branch>()
                                                                        .FindByConditionAsync(
                                                                            new GetBranchByIdWithoutIncludeSpecification(command.branchId), cancellationToken
                                                                        ) ?? throw new NotFoundException(
                                                                                [Messager.Create<Domain.Aggregates.Branches.Branch>().Message(MessageType.Found).Negative().BuildMessage()]
                                                                            );
            await unitOfWork.Repository<Domain.Aggregates.Branches.Branch>().DeleteAsync(branch);
            await unitOfWork.SaveAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
