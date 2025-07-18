using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Notifications
{
    public static class NotificationExtension
    {
        public static IServiceCollection AddNotification(this IServiceCollection services)
        {
            services.AddGrpc();
            services.AddGrpc();
            services.AddSignalR(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            });
            services.AddScoped<INotificationService, NotificationService>();

            services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();

            return services;
        }
    }
}
