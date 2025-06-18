using Contracts.Application.Common.Interfaces.Services.PubSub;
using Microsoft.Extensions.DependencyInjection;
using ProjectService_gRPC;

namespace Contracts.Infrastructure.PubSub
{
    public static class PubSubLogRegisterExtention
    {
        public static IServiceCollection PubSubLogClient(this IServiceCollection services)
        {
            services
                .AddGrpcClient<PubSubLogService.PubSubLogServiceClient>(o =>
                {
                    o.Address = new Uri("http://localhost:8443");
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    return handler;
                });

            services.AddSingleton<IPubSubLogService, PubSubLogServiceClient>();

            return services;
        }
    }
}
