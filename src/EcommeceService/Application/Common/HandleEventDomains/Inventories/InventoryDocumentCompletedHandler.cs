using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Utils;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Inventories.Events;
using Mediator;

namespace Application.Common.HandleEventDomains.Inventories;

public sealed class InventoryDocumentCompletedHandler
    : INotificationHandler<InventoryDocumentCompletedEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryDocumentCompletedHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async ValueTask Handle(
        InventoryDocumentCompletedEvent notification,
        CancellationToken cancellationToken
    )
    {
        var document = notification.InventoryDocument;

        var newEquipments = new List<Equipment>();

        foreach (var supplying in document.EquipmentSupplyings)
        {
            for (int i = 0; i < supplying.Quantity; i++)
            {
                var code = supplying.Code;
                if (i > 0)
                    code = supplying.Code + i;
                var equipment = new Equipment(
                    branchId: document.BranchId ?? 1,
                    name: supplying.Name,
                    code: code,
                    price: supplying.Price,
                    capacity: supplying.Capacity,
                    status: EquipmentStatus.Active,
                    description: document.Code,
                    lastMaintenanceDate: DateTimeOffset.UtcNow,
                    nextMaintenanceDate: DateTimeOffset.UtcNow.AddMonths(6)
                );

                newEquipments.Add(equipment);
            }
        }

        if (newEquipments.Any())
        {
            await _unitOfWork
                .Repository<Equipment>()
                .AddRangeAsync(newEquipments, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
        }
    }
}
