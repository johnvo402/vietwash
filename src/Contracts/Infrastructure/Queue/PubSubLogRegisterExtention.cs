using Contracts.Application.Common.Interfaces.Services.Notifications;
using Contracts.Application.Common.Interfaces.Services.PubSub;
using Contracts.Infrastructure.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Notification_Grpc;
using ProjectService_gRPC;

namespace Contracts.Infrastructure.PubSub
{
    public static class PubSubLogRegisterExtention
    {
        public static IServiceCollection PubSubLogClient(
            this IServiceCollection services,
            string? environmentName = "Development",
            IConfiguration? configuration = null
        )
        {
            services
                .AddGrpcClient<PubSubLogService.PubSubLogServiceClient>(o =>
                {
                    o.Address = new Uri(
                        configuration?["GrpcEndpoints:Project"] ?? (environmentName == "Development"
                            ? "http://localhost:8443"
                            : "http://project:8443")
                    );
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler();
                });

            services
                .AddGrpcClient<NotifyService.NotifyServiceClient>(o =>
                {
                    o.Address = new Uri(
                        configuration?["GrpcEndpoints:Notification"] ?? (environmentName == "Development"
                            ? "http://localhost:8444"
                            : "http://notification:8444")
                    );
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler();
                });

            services
                .AddSingleton<IPubSubLogService, PubSubLogServiceClient>()
                .AddSingleton<INotificationGrpc, NotificationClient>();

            return services;
        }
    }
}
