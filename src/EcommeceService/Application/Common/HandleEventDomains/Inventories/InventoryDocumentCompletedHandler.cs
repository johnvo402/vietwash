using Application.Common.Interfaces.Services.DistributedCache;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Jobs;
using Contracts.Application.Common.Interfaces.Services.Notifications;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Inventories.Events;
using Domain.Aggregates.Inventories.Spectifications;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.PubSubLogs;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Domain.Events;
using Domain.Events.Enums;
using Mediator;
using Notification_Grpc;
using Serilog;

namespace Application.Common.HandleEventDomains.Inventories;

public sealed class InventoryDocumentCompletedHandler
    : INotificationHandler<InventoryDocumentCompletedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationGrpc _notification;
    private readonly IPubSubFactory _queueFactory;
    private readonly ILogger _logger;

    public InventoryDocumentCompletedHandler(
        IUnitOfWork unitOfWork,
        INotificationGrpc notification,
        IPubSubFactory queueFactory,
        ILogger logger
    )
    {
        _unitOfWork = unitOfWork;
        _notification = notification;
        _queueFactory = queueFactory;
        _logger = logger;
    }

    public async ValueTask Handle(
        InventoryDocumentCompletedEvent notification,
        CancellationToken cancellationToken
    )
    {
        var document = await _unitOfWork
            .DynamicReadOnlyRepository<InventoryDocument>()
            .FindByConditionAsync(
                new GetInventoryDocumentByIdSpecification(notification.InventoryDocument.Id),
                cancellationToken
            );
        if (document == null)
        {
            return;
        }

        var newEquipments = new List<Equipment>();

        // Process supplyings and create consolidated fund events by supplier
        await ProcessSupplyings(document, newEquipments, cancellationToken);

        // Save new equipments if any
        if (newEquipments.Any())
        {
            await SaveEquipments(newEquipments, cancellationToken);
        }

        // Send notification to admins and managers
        await SendNotification(document, cancellationToken);
    }

    private async Task ProcessSupplyings(
        InventoryDocument document,
        List<Equipment> newEquipments,
        CancellationToken cancellationToken
    )
    {
        // Create equipment entries
        foreach (var supplying in document.EquipmentSupplyings)
        {
            for (int i = 0; i < supplying.Quantity; i++)
            {
                var code = i == 0 ? supplying.Code : $"{supplying.Code}{i}";
                var equipment = new Equipment(
                    branchId: document.BranchId ?? 1,
                    name: supplying.Name,
                    code: code,
                    price: supplying.Price,
                    status: EquipmentStatus.Active,
                    description: document.Code,
                    lastMaintenanceOrRepairDate: DateTimeOffset.UtcNow,
                    nextMaintenanceDate: DateTimeOffset.UtcNow.AddMonths(6)
                )
                {
                    Image = supplying.Image,
                };
                newEquipments.Add(equipment);
            }
        }

        // Combine supplyings into a common structure for grouping
        var supplierItems = document
            .EquipmentSupplyings.Select(s => new
            {
                SupplierId = (long?)s.SupplierId, // now nullable
                s.Supplier.Name,
                Amount = s.Price * s.Quantity,
            })
            .Concat(
                document
                    .ProductSupplyings.Where(x => x.SupplierId.HasValue && x.Supplier != null)
                    .Select(s => new
                    {
                        s.SupplierId,
                        s.Supplier!.Name,
                        Amount = s.Price * s.Quantity,
                    })
            );

        // Group by supplierId and calculate total amount
        var supplierGroups = supplierItems
            .GroupBy(s => s.SupplierId)
            .Select(g => new
            {
                SupplierId = g.Key,
                SupplierName = g.First().Name,
                TotalAmount = g.Sum(s => s.Amount),
            });

        // Publish one fund event per supplier
        foreach (var group in supplierGroups)
        {
            await PublishFundEvent(
                document,
                group.TotalAmount,
                (long)group.SupplierId!,
                group.SupplierName,
                cancellationToken
            );
        }
    }

    private async Task PublishFundEvent(
        InventoryDocument document,
        decimal amount,
        long supplierId,
        string supplierName,
        CancellationToken cancellationToken
    )
    {
        var fundEvent = new CreateFundEvent
        {
            TypeId = document.Type == InventoryType.Import ? "spend" : "income",
            ReferenceId = document.Id,
            Amount = amount,
            PaymentMethod = PaymentMethod.Cash,
            BranchId = (long)document.BranchId!,
            TransactionAt = (DateTimeOffset)document.TransactionAt!,
            BehaviorId = document.Type == InventoryType.Import ? 3 : 4,
            Metadata = new Dictionary<string, object>
            {
                ["code"] = document.Code,
                ["publicId"] = document.PublicId.ToString(),
                ["supplierId"] = supplierId,
                ["supplierName"] = supplierName,
                ["type"] = FundEventType.Inventory,
            },
            Point = 0,
            FundEventType = FundEventType.Inventory,
        };

        var success = await _queueFactory
            .GetPubSub(PubSubType.Origin)
            .PublishAsync(fundEvent, "CreateFundEvent");

        if (!success)
        {
            _logger.Error(
                "CreateFundEventHandler: {@ReferenceId} enqueue failed for SupplierId: {@SupplierId}, SupplierName: {@SupplierName}",
                fundEvent.ReferenceId,
                supplierId,
                supplierName
            );
        }
    }

    private async Task SaveEquipments(
        List<Equipment> equipments,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await _unitOfWork.Repository<Equipment>().AddRangeAsync(equipments, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task SendNotification(
        InventoryDocument document,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var users = await _unitOfWork
                .DynamicReadOnlyRepository<User>()
                .ListAsync(
                    new ListUserByRoleIncludeSpecification(["ADMIN", "MANAGER"]),
                    new QueryParamRequest(),
                    x => new OnlyId { Id = x.Id },
                    cancellationToken
                );

            var branchName = await _unitOfWork
                .Repository<BranchUser>()
                .FindByConditionAsync(
                    x => x.BranchId == document.BranchId,
                    x => new OnlyId { Name = x.BranchName },
                    cancellationToken
                );

            var notifySend = new SendNotificationRequest
            {
                TemplateId = "inventory_import",
                Time = document.TransactionAt.ToString(),
                Parameters =
                {
                    ["code"] = document.Code,
                    ["branch_name"] = branchName?.Name ?? string.Empty,
                },
                Data =
                {
                    ["import_id"] = document.Id.ToString(),
                    ["publicId"] = document.PublicId.ToString(),
                },
            };
            notifySend.UserIds.AddRange(users.Select(x => x.Id.ToString()));

            await _notification.SendNotifyAsync(notifySend, cancellationToken);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
