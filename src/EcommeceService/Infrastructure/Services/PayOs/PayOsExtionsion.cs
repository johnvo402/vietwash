using Application.Feature.Orders.Queries.GetLinkPayment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Net.payOS;

namespace Infrastructure.Services.PayOs
{
    public static class PayOsExtension
    {
        public static IServiceCollection AddPayOs(
            this IServiceCollection services,
            IConfiguration config,
            string? environmentName = "Development"
        )
        {
            IConfigurationSection section = config.GetSection(nameof(PayOsSetting));
            PayOsSetting payOsSetting = section.Get<PayOsSetting>() ?? new PayOsSetting();
            IReadOnlyList<string> errors = PayOsSettingValidator.GetErrors(payOsSetting,
                !string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase));
            if (errors.Count != 0)
                throw new OptionsValidationException(
                    nameof(PayOsSetting),
                    typeof(PayOsSetting),
                    errors
                );

            services.Configure<PayOsSetting>(section);
            services.AddSingleton(payOsSetting);
            services.AddSingleton<IOrderPaymentSettings>(payOsSetting);

            if (payOsSetting.IsEnabled)
            {
                var payOS = new PayOS(
                    payOsSetting.ClientId!,
                    payOsSetting.ApiKey!,
                    payOsSetting.ChecksumKey!
                );

                services.AddSingleton(payOS);
                services.AddSingleton<IOrderPaymentLinkClient, OrderPaymentLinkClient>();
                services.AddSingleton<IOrderPaymentWebhookVerifier, PayOsWebhookVerifier>();
            }
            else
            {
                services.AddSingleton<IOrderPaymentLinkClient, UnavailableOrderPaymentLinkClient>();
                services.AddSingleton<
                    IOrderPaymentWebhookVerifier,
                    UnavailablePayOsWebhookVerifier
                >();
            }

            return services;
        }
    }
}
