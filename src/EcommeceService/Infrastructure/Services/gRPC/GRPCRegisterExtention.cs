using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
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

    public static IEndpointRouteBuilder UseGrpcEndpoints(this IEndpointRouteBuilder endpoints)
    {
       
            // endpoints.MapGrpcService<PubSubLogServiceHandler>();
            endpoints.MapGrpcReflectionService();        

        return endpoints;
    }
}