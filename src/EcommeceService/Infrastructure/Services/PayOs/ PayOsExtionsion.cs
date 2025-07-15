using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Net.payOS;

namespace Infrastructure.Services.PayOs
{
    public static class PayOsExtionsion
    {
        public static IServiceCollection AddPayOs(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            services.Configure<PayOsSetting>(config.GetSection(nameof(PayOsSetting)));
            var payOsSetting = config.GetSection(nameof(PayOsSetting)).Get<PayOsSetting>();
            PayOS payOS = new PayOS(
                payOsSetting!.PAYOS_CLIENT_ID,
                payOsSetting!.PAYOS_API_KEY,
                payOsSetting!.PAYOS_CHECKSUM_KEY
            );
            services.AddSingleton(payOS);

            return services;
        }
    }
}
