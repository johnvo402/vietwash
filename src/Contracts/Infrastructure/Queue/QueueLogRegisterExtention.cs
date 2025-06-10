using Contracts.Application.Common.Interfaces.Services.Queue;
using Microsoft.Extensions.DependencyInjection;
using ProjectService_gRPC;


namespace Contracts.Infrastructure.Queue
{
    public static class QueueLogRegisterExtention
    {

        public static IServiceCollection QueueLogClient(this IServiceCollection services)
        {
            services.AddGrpcClient<QueueLogService.QueueLogServiceClient>(o =>
            {
                o.Address = new Uri("http://localhost:8443");

            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                return handler;
            });

            services.AddSingleton<IQueueLogService, QueueLogServiceClient>();

            return services;
        }
    }

}

