using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Suppliers;
using Mediator;

namespace Application.Feature.Suppliers.Command.Update
{
    public class UpdateSupplierHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateSupplierCommand, Result>
    {
        public async ValueTask<Result> Handle(
            UpdateSupplierCommand request,
            CancellationToken cancellationToken
        )
        {
            Supplier? existingSupplier = await unitOfWork
                .Repository<Supplier>()
                .FindByConditionAsync(
                    s => s.Id == request.SupplierId && !s.Disable,
                    cancellationToken
                );
            if (existingSupplier == null)
            {
                return Result.Failure(
                    new NotFoundError(
                        "Supplier not found",
                        Messager
                            .Create<Supplier>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }

            existingSupplier.FromModel(request.Supplier);
            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                await unitOfWork.Repository<Supplier>().UpdateAsync(existingSupplier);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);

                return Result.Success();
            }
            catch
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
