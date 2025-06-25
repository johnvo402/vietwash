using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Branches;
using Domain.Aggregates.Branches.Specifications;
using Mediator;

namespace Application.Features.Branches.Commands.Update
{
    public class UpdateBranchHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateBranchCommand, Result>
    {
        public async ValueTask<Result> Handle(
            UpdateBranchCommand request,
            CancellationToken cancellationToken
        )
        {
            Branch? branch = await unitOfWork
                .DynamicReadOnlyRepository<Domain.Aggregates.Branches.Branch>()
                .FindByConditionAsync(
                    new GetBranchByIdWithoutIncludeSpecification(request.BranchId),
                    cancellationToken
                );
            if (branch == null)
            {
                return Result.Failure(
                    new NotFoundError(
                        "branch not found",
                        Messager
                            .Create<Branch>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }
            branch.MapToEntity(request.Branch!);
            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );
                await unitOfWork.Repository<Branch>().UpdateAsync(branch);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
