using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.InventoryDocuments.Commands.Delete
{
    public class DeleteInventoryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteInventoryCommand, Result>
    {
        public async ValueTask<Result> Handle(
            DeleteInventoryCommand request,
            CancellationToken cancellationToken
        )
        {
            var inv = await unitOfWork
                .Repository<InventoryDocument>()
                .QueryAsync(x => x.Id == request.InventoryId && x.Status == InventoryStatus.Pending)
                .FirstOrDefaultAsync(cancellationToken);

            if (inv == null)
            {
                return Result.Failure(
                    new NotFoundError(
                        "InventoryDocument not found",
                        Messager
                            .Create<InventoryDocument>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }

            try
            {
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
                await unitOfWork.Repository<InventoryDocument>().DeleteAsync(inv);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (System.Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
