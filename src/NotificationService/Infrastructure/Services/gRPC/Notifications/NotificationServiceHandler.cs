using Application.Features.Notifications.Commands.SendNotifications;
using Grpc.Core;
using Mediator;
using Notification_Grpc;
using Serilog;

namespace Infrastructure.Services.gRPC.Notifications
{
    public class NotificationServiceHandler(ISender sender, ILogger _logger)
        : NotifyService.NotifyServiceBase
    {
        public override async Task<SendNotificationResponse> SendNotification(
            SendNotificationRequest request,
            ServerCallContext context
        )
        {
            try
            {
                var command = new SendNotificationCommand
                {
                    UserIds = request.UserIds.ToList(),
                    Parameters =
                        request.Parameters?.ToDictionary(entry => entry.Key, entry => entry.Value)
                        ?? null,
                    TemplateId = request.TemplateId,
                    Data =
                        request.Data?.ToDictionary(entry => entry.Key, entry => entry.Value)
                        ?? null,
                    Time = request.Time,
                };

                var result = await sender.Send(command);
                return new SendNotificationResponse { Success = result.IsSuccess };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed queue log.");
                return new SendNotificationResponse { Success = false };
            }
        }
    }
}
