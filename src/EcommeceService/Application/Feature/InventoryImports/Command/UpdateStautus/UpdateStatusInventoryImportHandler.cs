using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Command.UpdateStatus;
using AutoMapper;
using Domain.Aggregates.Inventories.Specifications;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Orders.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;


namespace Application.Feature.InventoryImports.Command.UpdateStautus
{
    public class UpdateStatusInventoryImportHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper
    )
        : IRequestHandler<UpdateStatusInventoryImportCommand, UpdateStatusInventoryImportResponse>
    {
        public async ValueTask<UpdateStatusInventoryImportResponse> Handle(UpdateStatusInventoryImportCommand request, CancellationToken cancellationToken)
        {
            try
            {
                InventoryDocument? existingInventoryImport = await unitOfWork.Repository<InventoryDocument>().FindByConditionAsync(new GetInventoryDocumentByIdSpec(request.InventoryImportId), cancellationToken)
                        ?? throw new NotFoundException(
                 [Messager.Create<InventoryDocument>().Message(MessageType.Found).Negative().BuildMessage()]
             );


                if (request.Status.HasValue)
                {
                    if (request.Status.Value < existingInventoryImport.Status)
                        throw new BadRequestException(
                            [Messager.Create<InventoryDocument>().Property(x => x.Status).Message(MessageType.Valid).Negative().Build()]);

                    existingInventoryImport.Status = request.Status.Value;
                }
                using var transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

                await unitOfWork.Repository<InventoryDocument>().UpdateAsync(existingInventoryImport);
                await unitOfWork.SaveAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return new UpdateStatusInventoryImportResponse
                {
                    Message = "Inventory import status updated successfully"
                };
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }

        }
    }
}
