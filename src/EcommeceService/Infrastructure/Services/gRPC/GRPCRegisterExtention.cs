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

    public static IApplicationBuilder UseGrpcEndpoints(this IApplicationBuilder app)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            // endpoints.MapGrpcService<QueueLogServiceHandler>();
            endpoints.MapGrpcReflectionService();        

        });

        return app;
    }
}