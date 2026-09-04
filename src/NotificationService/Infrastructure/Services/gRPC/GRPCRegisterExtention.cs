using Infrastructure.Services.gRPC.Notifications;
using Infrastructure.Services.Notifications;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.gRPC;

public static class GRPCRegisterExtension
{
    public static IServiceCollection AddGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc();
        services.AddGrpcReflection();
        return services;
    }

    public static void UseGrpcHubEndpoints(this WebApplication app)
    {
        app.MapGrpcService<NotificationServiceHandler>().AllowAnonymous();
        app.MapGrpcReflectionService().AllowAnonymous();
    }
}
