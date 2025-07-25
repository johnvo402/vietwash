using Contracts.Application.Common.Interfaces.Services.Notifications;
using Notification_Grpc;

namespace Contracts.Infrastructure.Notifications
{
    public class NotificationClient(NotifyService.NotifyServiceClient client) : INotificationGrpc
    {
        public async Task<bool> SendNotifyAsync(
            SendNotificationRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var response = await client.SendNotificationAsync(
                request,
                cancellationToken: cancellationToken
            );
            return response.Success;
        }
    }
}
