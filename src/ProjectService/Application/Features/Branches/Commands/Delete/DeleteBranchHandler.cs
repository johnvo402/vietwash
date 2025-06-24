using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Branches;
using Domain.Aggregates.Branches.Specifications;
using Mediator;

namespace Application.Features.Branches.Commands.Delete
{
    public class DeleteBranchHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteBranchCommand, Result>
    {
        public async ValueTask<Result> Handle(
            DeleteBranchCommand command,
            CancellationToken cancellationToken
        )
        {
            Branch? branch = await unitOfWork
                .DynamicReadOnlyRepository<Domain.Aggregates.Branches.Branch>()
                .FindByConditionAsync(
                    new GetBranchByIdWithoutIncludeSpecification(command.branchId),
                    cancellationToken
                );
            if (branch == null)
            {
                return Result.Failure(
                    new NotFoundError(
                        "Branch not found",
                        Messager
                            .Create<Domain.Aggregates.Branches.Branch>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }
            if (branch.Disable != true)
            {
                branch.Disable = true;
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );
                try
                {
                    await unitOfWork
                        .Repository<Domain.Aggregates.Branches.Branch>()
                        .UpdateAsync(branch);
                    await unitOfWork.SaveAsync(cancellationToken);
                    await unitOfWork.CommitAsync(cancellationToken);
                }
                catch (Exception)
                {
                    await unitOfWork.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            return Result.Success();
        }
    }
}
