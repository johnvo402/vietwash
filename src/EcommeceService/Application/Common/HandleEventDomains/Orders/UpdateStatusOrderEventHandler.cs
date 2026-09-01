using Application.Common.Interfaces.UnitOfWorks;
using Application.Jobs;
using Contracts.Application.Common.Interfaces.Services.Notifications;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.Users;
using Mediator;
using Notification_Grpc;

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
            switch (order.Status)
            {
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
}
