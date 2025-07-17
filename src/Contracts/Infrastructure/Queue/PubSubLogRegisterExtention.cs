using Contracts.Application.Common.Interfaces.Services.Notifications;
using Contracts.Application.Common.Interfaces.Services.PubSub;
using Contracts.Infrastructure.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Notification_Grpc;
using ProjectService_gRPC;

namespace Contracts.Infrastructure.PubSub
{
    public static class PubSubLogRegisterExtention
    {
        public static IServiceCollection PubSubLogClient(
            this IServiceCollection services,
            string? environmentName = "Development"
        )
        {
            services
                .AddGrpcClient<PubSubLogService.PubSubLogServiceClient>(o =>
                {
                    o.Address = new Uri(
                        environmentName!.CompareTo("Development") == 0
                            ? "http://localhost:8443"
                            : "http://project:8443"
                    );
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    return handler;
                });

            services
                .AddGrpcClient<NotifyService.NotifyServiceClient>(o =>
                {
                    o.Address = new Uri(
                        environmentName!.CompareTo("Development") == 0
                            ? "http://localhost:8444"
                            : "http://notification:8444"
                    );
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    return handler;
                });

            services
                .AddSingleton<IPubSubLogService, PubSubLogServiceClient>()
                .AddSingleton<INotificationGrpc, NotificationClient>();

            return services;
        }
    }
}
