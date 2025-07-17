using Notification_Grpc;

namespace Contracts.Application.Common.Interfaces.Services.Notifications
{
    public interface INotificationGrpc
    {
        Task<bool> SendNotifyAsync(
            SendNotificationRequest request,
            CancellationToken cancellationToken
        );
    }
}
