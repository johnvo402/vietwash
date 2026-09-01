using Application.Common.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public static class CurrentAccountRegistration
{
    public static IServiceCollection AddCurrentAccount(this IServiceCollection services) =>
        services.AddScoped<ICurrentAccount, CurrentUserService>();
}
