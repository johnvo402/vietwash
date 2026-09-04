using System.Reflection;
using Application.Common.Auth;
using Application.Common.Behaviors;
using Application.Jobs;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        Assembly currentAssembly = Assembly.GetExecutingAssembly();

        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Stop;

        return services
            .AddMediator(option => option.ServiceLifetime = ServiceLifetime.Scoped)
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ErrorLoggingBehaviour<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformaceBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ProcessImagePathBehavior<,>))
            .AddValidatorsFromAssembly(currentAssembly)
            .AddSingleton<IAuthorizationPolicyProvider, AuthorizePolicyProvider>()
            .AddSingleton<IAuthorizationHandler, AuthorizeHandler>()
            .AddScoped<UpdateUserStatusJob>();
        ;
    }
}
