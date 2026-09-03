using Application.Common.Interfaces.UnitOfWorks;
using Application.Jobs;
using Contracts.Application.Common.Interfaces.Services.Notifications;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.Users;
using Mediator;
using Notification_Grpc;
using Serilog;

namespace Application.Common.HandleEventDomains.Orders
{
    public class UpdateStatusOrderEventHandler : INotificationHandler<UpdateStatusOrderEvent>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly INotificationGrpc _notification;
        private readonly ILogger _logger;

        public UpdateStatusOrderEventHandler(
            IUnitOfWork _unitOfWork,
            INotificationGrpc notification,
            ILogger logger
        )
        {
            this._unitOfWork = _unitOfWork;
            _notification = notification;
            _logger = logger;
        }

        public async ValueTask Handle(
            UpdateStatusOrderEvent notification,
            CancellationToken cancellationToken
        )
        {
            var order = notification.Order;
            if (order.Status != OrderStatus.Processed || !order.CustomerId.HasValue)
                return;

            // Notification preparation and delivery are optional. Neither may roll back
            // the primary order transition or equipment release in the caller's transaction.
            try
            {
                var branchName = await _unitOfWork
                    .Repository<BranchUser>()
                    .FindByConditionAsync(
                        x => x.BranchId == order.BranchId,
                        x => new OnlyId { Name = x.BranchName },
                        cancellationToken: cancellationToken
                    );
                var notifySend = new SendNotificationRequest
                {
                    TemplateId = "laundry_processed",
                    Time = order.CreatedAt.ToString(),
                };
                notifySend.Parameters["order_code"] = order.Code;
                notifySend.Parameters["branch_name"] = branchName?.Name ?? $"#{order.BranchId}";
                notifySend.Data["order_id"] = order.Id.ToString();
                notifySend.Data["publicId"] = order.PublicId.ToString();
                notifySend.UserIds.Add(order.CustomerId.Value.ToString());

                if (!await _notification.SendNotifyAsync(notifySend, cancellationToken))
                    _logger.Warning(
                        "Processed-order notification was not delivered. OrderId: {OrderId}, OrderCode: {OrderCode}, BranchId: {BranchId}, CustomerId: {CustomerId}, Status: {Status}",
                        order.Id, order.Code, order.BranchId, order.CustomerId, order.Status
                    );
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Failed to send processed-order notification. OrderId: {OrderId}, OrderCode: {OrderCode}, BranchId: {BranchId}, CustomerId: {CustomerId}, Status: {Status}",
                    order.Id, order.Code, order.BranchId, order.CustomerId, order.Status
                );
            }
        }
    }
}
