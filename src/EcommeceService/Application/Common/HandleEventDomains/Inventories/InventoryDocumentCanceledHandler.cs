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
    : INotificationHandler<InventoryDocumentCanceledEvent>
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
        var document = notification.InventoryDocument;
        logger.Information("InventoryDocumentCanceledHandler: {@Id}", document.Code);
        List<string> codes = new List<string>();
        foreach (var supplying in document.EquipmentSupplyings)
        {
            for (int i = 0; i < supplying.Quantity; i++)
            {
                var code = supplying.Code;
                if (i > 0)
                    code = supplying.Code + i;

                codes.Add(code);
            }
        }

        if (codes.Any())
        {
            var equipments = await _unitOfWork
                .DynamicReadOnlyRepository<Equipment>()
                .ListAsync(
                    new ListEquipmentByCodeSpecification(codes),
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
                logger.Error("InventoryDocumentCanceledHandler: {@Id}", document.Code);
                await _unitOfWork.RollbackAsync(cancellationToken);
            }
        }
    }
}
