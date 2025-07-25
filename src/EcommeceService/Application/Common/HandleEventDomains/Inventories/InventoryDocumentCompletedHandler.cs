using Application.Common.Interfaces.UnitOfWorks;
using Application.Jobs;
using Contracts.Application.Common.Interfaces.Services.Notifications;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Inventories.Events;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;
using Notification_Grpc;

namespace Application.Common.HandleEventDomains.Inventories;

public sealed class InventoryDocumentCompletedHandler
    : INotificationHandler<InventoryDocumentCompletedEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly INotificationGrpc _notification;

    public InventoryDocumentCompletedHandler(IUnitOfWork unitOfWork, INotificationGrpc notification)
    {
        _unitOfWork = unitOfWork;
        _notification = notification;
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
                    status: EquipmentStatus.Active,
                    description: document.Code,
                    lastMaintenanceOrRepairDate: DateTimeOffset.UtcNow,
                    nextMaintenanceDate: DateTimeOffset.UtcNow.AddMonths(6)
                );
                equipment.Image = supplying.Image;

                newEquipments.Add(equipment);
            }
        }
        var users = await _unitOfWork
            .DynamicReadOnlyRepository<User>()
            .ListAsync(
                new ListUserByRoleIncludeSpecification(["ADMIN", "MANAGER"]),
                new QueryParamRequest(),
                x => new OnlyId { Id = x.Id },
                cancellationToken: default
            );
        var branchName = await _unitOfWork
            .Repository<BranchUser>()
            .FindByConditionAsync(
                x => x.BranchId == document.BranchId,
                x => new OnlyId { Name = x.BranchName },
                cancellationToken: default
            );
        if (newEquipments.Any())
        {
            try
            {
                _ = await _unitOfWork.BeginTransactionAsync(cancellationToken);
                await _unitOfWork
                    .Repository<Equipment>()
                    .AddRangeAsync(newEquipments, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
        try
        {
            var notifySend = new SendNotificationRequest { TemplateId = "inventory_import" };
            notifySend.Parameters["code"] = document.Code;
            notifySend.Parameters["branch_name"] = branchName?.Name;

            notifySend.Data["import_id"] = document.Id.ToString();
            notifySend.Data["publicId"] = document.PublicId.ToString();
            var userIds = users.Select(x => x.Id.ToString()).ToList();
            notifySend.UserIds.AddRange(userIds);
            await _notification.SendNotifyAsync(notifySend, cancellationToken);
        }
        catch (System.Exception)
        {
            throw;
        }
    }
}
