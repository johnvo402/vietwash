using Application.Feature.Orders.Queries.GetLinkPayment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Net.payOS;

namespace Infrastructure.Services.PayOs
{
    public static class PayOsExtension
    {
        public static IServiceCollection AddPayOs(this IServiceCollection services, IConfiguration config)
        {
            // Đăng ký cấu hình
            services.Configure<PayOsSetting>(config.GetSection(nameof(PayOsSetting)));

            // Bind và validate 1 lần
            var payOsSetting = config.GetSection(nameof(PayOsSetting)).Get<PayOsSetting>();

            if (payOsSetting?.IsEnabled == true)
            {
                var payOS = new PayOS(
                    payOsSetting.ClientId,
                    payOsSetting.ApiKey,
                    payOsSetting.ChecksumKey
                );

                services.AddSingleton(payOS);
                services.AddSingleton<IOrderPaymentLinkClient, OrderPaymentLinkClient>();
            }

            return services;
        }
    }
}
