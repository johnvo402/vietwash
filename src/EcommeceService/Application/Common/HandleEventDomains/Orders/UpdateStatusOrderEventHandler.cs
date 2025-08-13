using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Inventories;
using Application.Feature.InventoryDocuments.Commands.Create;
using Application.Jobs;
using Contracts.Application.Common.Interfaces.Services.Notifications;
using Contracts.Utils;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.Users;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notification_Grpc;
using Wangkanai.Extensions;

namespace Application.Common.HandleEventDomains.Orders
{
    public class UpdateStatusOrderEventHandler : INotificationHandler<UpdateStatusOrderEvent>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly INotificationGrpc _notification;

        public UpdateStatusOrderEventHandler(
            IUnitOfWork _unitOfWork,
            INotificationGrpc notification
        )
        {
            this._unitOfWork = _unitOfWork;
            _notification = notification;
        }

        public async ValueTask Handle(
            UpdateStatusOrderEvent notification,
            CancellationToken cancellationToken
        )
        {
            var order = notification.Order;
            var equipmentsToUpdate = new List<Equipment>();

            switch (order.Status)
            {
                case OrderStatus.InProgress:
                {
                    var orderInProgress = await _unitOfWork
                        .Repository<Order>()
                        .QueryAsync(o => o.Id == order.Id)
                        .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.UnitRelation)
                        .ThenInclude(ur => ur.AsUnitRelation)
                        .ThenInclude(sr => sr.UnitProduct)
                        .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.UnitRelation)
                        .ThenInclude(ur => ur.AsUnitRelation)
                        .ThenInclude(sr => sr.BranchProduct)
                        .FirstAsync(cancellationToken);

                    if (orderInProgress == null)
                        break;
                    var exportDocument = new InventoryDocument();

                    var issueLines = orderInProgress
                        .OrderItems.SelectMany(oi =>
                        {
                            var unitRelation = oi.UnitRelation;
                            if (unitRelation == null)
                            {
                                return Enumerable.Empty<IssueLine>();
                            }

                            if (!unitRelation.AsUnitRelation.Any())
                            {
                                return Enumerable.Empty<IssueLine>();
                            }

                            return unitRelation.AsUnitRelation.Select(sr =>
                            {
                                decimal serviceFactor = unitRelation.BaseUnit
                                    ? 1m
                                    : (decimal)unitRelation.Multiple;
                                decimal requireQty = sr.Quantity * serviceFactor * oi.Quantity;

                                return new IssueLine(
                                    sr.ProductId,
                                    sr.UnitProductId,
                                    requireQty,
                                    sr.BranchProduct.CapitalPrice
                                );
                            });
                        })
                        .GroupBy(x => new { x.BranchProductId, x.UnitRelationId })
                        .Select(g => new IssueLine(
                            g.Key.BranchProductId,
                            g.Key.UnitRelationId,
                            g.Sum(x => x.Quantity),
                            g.First().Price
                        ))
                        .ToList();

                    if (issueLines.Any())
                    {
                        decimal totalProductAmount = issueLines.Sum(x => x.Price * x.Quantity);
                        exportDocument = new InventoryDocument(
                            code: Generator.GenerateCode("XH", 6),
                            amount: totalProductAmount,
                            type: InventoryType.Export,
                            branchId: order.BranchId,
                            note: $"Phiếu xuất cho đơn hàng #{order.Code}"
                        )
                        {
                            TransactionAt = DateTimeOffset.UtcNow,
                        };

                        exportDocument.ProductSupplyings.AddRangeSafe(
                            issueLines
                                .Select(x => new ProductSupplying
                                {
                                    ProductId = x.BranchProductId,
                                    UnitRelationId = x.UnitRelationId,
                                    Price = x.Price,
                                    SupplierId = null,
                                    Quantity = -x.Quantity,
                                })
                                .ToList()
                        );
                    }

                    foreach (var x in notification.Order.OrderEquipments)
                    {
                        var equipment = await _unitOfWork
                            .Repository<Equipment>()
                            .FindByIdAsync(x.EquipmentId, cancellationToken);

                        if (equipment != null)
                        {
                            equipment.Using = true;
                            equipmentsToUpdate.Add(equipment);
                        }
                    }
                    try
                    {
                        await _unitOfWork.BeginTransactionAsync(cancellationToken);
                        if (equipmentsToUpdate.Any())
                        {
                            await _unitOfWork
                                .Repository<Equipment>()
                                .UpdateRangeAsync(equipmentsToUpdate);
                            await _unitOfWork.SaveAsync(cancellationToken);
                        }
                        await _unitOfWork
                            .Repository<InventoryDocument>()
                            .AddAsync(exportDocument, cancellationToken);
                        await _unitOfWork.SaveAsync(cancellationToken);
                        exportDocument.UpdateStatus(InventoryStatus.Completed);
                        await _unitOfWork.SaveAsync(cancellationToken);
                        await _unitOfWork.CommitAsync(cancellationToken);
                    }
                    catch (System.Exception)
                    {
                        await _unitOfWork.RollbackAsync(cancellationToken);
                        throw;
                    }
                    break;
                }

                case OrderStatus.Processed:

                    foreach (var x in notification.Order.OrderEquipments)
                    {
                        var equipment = await _unitOfWork
                            .Repository<Equipment>()
                            .FindByIdAsync(x.EquipmentId, cancellationToken);

                        if (equipment != null)
                        {
                            equipment.Using = false;
                            equipmentsToUpdate.Add(equipment);
                        }
                    }
                    try
                    {
                        await _unitOfWork.BeginTransactionAsync(cancellationToken);
                        if (equipmentsToUpdate.Any())
                        {
                            await _unitOfWork
                                .Repository<Equipment>()
                                .UpdateRangeAsync(equipmentsToUpdate);
                            await _unitOfWork.SaveAsync(cancellationToken);
                            await _unitOfWork.CommitAsync(cancellationToken);
                        }
                    }
                    catch (System.Exception)
                    {
                        await _unitOfWork.RollbackAsync(cancellationToken);
                        throw;
                    }
                    var branchName = await _unitOfWork
                        .Repository<BranchUser>()
                        .FindByConditionAsync(
                            x => x.BranchId == order.BranchId,
                            x => new OnlyId { Name = x.BranchName },
                            cancellationToken: default
                        );
                    try
                    {
                        var notifySend = new SendNotificationRequest
                        {
                            TemplateId = "laundry_processed",
                            Time = order.CreatedAt.ToString(),
                        };
                        notifySend.Parameters["order_code"] = order.Code;
                        notifySend.Parameters["branch_name"] = branchName?.Name;

                        notifySend.Data["order_id"] = order.Id.ToString();
                        notifySend.Data["publicId"] = order.PublicId.ToString();
                        if (order.CustomerId != null)
                        {
                            notifySend.UserIds.Add(order.CustomerId.Value.ToString());
                            await _notification.SendNotifyAsync(notifySend, cancellationToken);
                        }
                    }
                    catch (System.Exception)
                    {
                        throw;
                    }
                    break;
                default:
                    return;
            }
        }
    }

    public record IssueLine(
        long BranchProductId,
        long UnitRelationId,
        decimal Quantity,
        decimal Price
    );
}
