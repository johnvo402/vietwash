using System.Data.Common;
using System.Threading.Tasks;
using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Warehouses;
using Domain.Aggregates.Warehouses.Specifications;
using Mediator;

namespace Application.Features.Warehouses.Commands.Update
{
    public class UpdateWarehouseHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateWarehouseCommand, Result>
    {
        public async ValueTask<Result> Handle(
            UpdateWarehouseCommand request,
            CancellationToken cancellationToken
        )
        {
            Warehouse? warehouse = await unitOfWork
                .DynamicReadOnlyRepository<Warehouse>()
                .FindByConditionAsync(
                    new GetWarehouseByIdWithoutIncludeSpecification(request.WarehouseId),
                    cancellationToken
                );
            if (warehouse == null)
            {
                return Result.Failure(
                    new NotFoundError(
                        "Warehouse not found",
                        Messager
                            .Create<Warehouse>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }
            warehouse.UpdateWarehouse(request.Warehouse!);

            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );
                await unitOfWork.Repository<Warehouse>().UpdateAsync(warehouse);
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
