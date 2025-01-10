


using FluentValidation;
using Micro.Shared.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

                options.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
                options.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));
            services.AddAutoMapper(typeof(MapperProfile).Assembly);
            return services;
        }
    }
}
