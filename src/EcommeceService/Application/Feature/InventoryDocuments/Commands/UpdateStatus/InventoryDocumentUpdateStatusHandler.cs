using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Spectifications;
using Mediator;

namespace Application.Feature.InventoryDocuments.Commands.UpdateStatus
{
    public class InventoryDocumentUpdateStatusHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<InventoryDocumentUpdateStatusCommand, Result>
    {
        public async ValueTask<Result> Handle(
            InventoryDocumentUpdateStatusCommand request,
            CancellationToken cancellationToken
        )
        {
            var result = await unitOfWork
                .DynamicReadOnlyRepository<InventoryDocument>()
                .FindByConditionAsync(
                    new GetInventoryDocumentByIdSpecification(request.Id),
                    cancellationToken
                );
            if (result == null)
            {
                return Result.Failure(
                    new NotFoundError(
                        "Inventory document not found",
                        Messager
                            .Create<InventoryDocument>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }
            result.ToUpdateStatusInventoryDocument(request.ModelStatus);

            try
            {
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
                await unitOfWork.Repository<InventoryDocument>().UpdateAsync(result);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }

            return Result.Success();
        }
    }
}
