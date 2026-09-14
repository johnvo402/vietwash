using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Equipments.Specifications;
using Domain.Aggregates.Inventories.Events;
using Mediator;
using Serilog;

namespace Application.Common.HandleEventDomains.Inventories;

public sealed class InventoryDocumentCanceledHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger logger;

    public InventoryDocumentCanceledHandler(ILogger logger, IUnitOfWork unitOfWork)
    {
        this.logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask Handle(
        InventoryDocumentCanceledEvent notification,
        CancellationToken cancellationToken
    )
    {
        logger.Information(
            "InventoryDocumentCanceledHandler: {@Id}",
            notification.DocumentCode
        );

        if (notification.EquipmentCodes.Count != 0)
        {
            var equipments = await _unitOfWork
                .DynamicRepository<Equipment>()
                .ListAsync(
                    new ListEquipmentByCodeSpecification(notification.EquipmentCodes.ToList()),
                    new QueryParamRequest(),
                    cancellationToken
                );
            try
            {
                _ = await _unitOfWork.BeginTransactionAsync(cancellationToken);
                await _unitOfWork.Repository<Equipment>().DeleteRangeAsync(equipments);
                await _unitOfWork.SaveAsync(cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                logger.Error(
                    "InventoryDocumentCanceledHandler: {@Id}",
                    notification.DocumentCode
                );
                await _unitOfWork.RollbackAsync(cancellationToken);
            }
        }
    }
}
