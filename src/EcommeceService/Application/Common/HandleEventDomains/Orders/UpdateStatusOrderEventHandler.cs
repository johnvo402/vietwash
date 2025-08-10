using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Inventories;
using Application.Feature.InventoryDocuments.Commands.Create;
using Application.Jobs;
using Contracts.Application.Common.Interfaces.Services.Notifications;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.Users;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notification_Grpc;

namespace Application.Common.HandleEventDomains.Orders
{
    public class UpdateStatusOrderEventHandler : INotificationHandler<UpdateStatusOrderEvent>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly INotificationGrpc _notification;
        private readonly ISender _sender;

        public UpdateStatusOrderEventHandler(
            IUnitOfWork unitOfWork,
            INotificationGrpc notification,
            ISender sender
        )
        {
            _unitOfWork = unitOfWork;
            _notification = notification;
            _sender = sender;
        }

        public async ValueTask Handle(
            UpdateStatusOrderEvent notification,
            CancellationToken cancellationToken
        )
        {
            var order = notification.Order;
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

                    var issueLines = orderInProgress
                        .OrderItems.SelectMany(oi =>
                            oi.UnitRelation.AsUnitRelation.Select(sr =>
                            {
                                decimal serviceFactor = oi.UnitRelation.BaseUnit
                                    ? 1m
                                    : (decimal)oi.UnitRelation.Multiple;
                                decimal requireQty = sr.Quantity * serviceFactor * oi.Quantity;

                                return new
                                {
                                    BranchProductId = sr.BranchProduct.Id,
                                    UnitRelationId = sr.UnitProduct.Id, // đơn vị xuất kho
                                    Quantity = requireQty,
                                    Price = sr.UnitProduct.Price, // ✅ lấy price từ UnitRelation của product
                                };
                            })
                        )
                        .GroupBy(x => new { x.BranchProductId, x.UnitRelationId })
                        .Select(g => new
                        {
                            g.Key.BranchProductId,
                            g.Key.UnitRelationId,
                            Quantity = g.Sum(x => x.Quantity),
                            Price = g.First().Price, // cùng UnitRelation nên price đồng nhất
                        })
                        .ToList();

                    var doc = new CreateInventoryDocumentCommand
                    {
                        BranchId = orderInProgress.BranchId,
                        Type = InventoryType.Export,
                        Note = $"#{orderInProgress.Code}",
                        TransactionAt = DateTimeOffset.UtcNow,
                        ProductSupplyings =
                        [
                            .. issueLines.Select(x => new ProductSupplyingModel
                            {
                                ProductId = x.BranchProductId,
                                UnitRelationId = x.UnitRelationId,
                                Price = x.Price, // ✅ set price theo UnitProduct.Price
                                SupplierId = null,
                                Quantity = -(int)Math.Ceiling(x.Quantity),
                            }),
                        ],
                    };

                    await _sender.Send(doc, cancellationToken);
                    break;
                }

                case OrderStatus.Processed:
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
}
