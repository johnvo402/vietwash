using Contracts.Application.Common.Interfaces.Services.Encryptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts.Infrastructure.Services.Encryptions
{
    public static class EncryptionExtension
    {
        public static IServiceCollection AddEncryption(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            services.Configure<EncryptionOptions>(
                config.GetSection($"SecuritySettings:{nameof(EncryptionOptions)}")
            );
            services.AddSingleton<IEncryptionService, AesEncryptionService>();
            return services;
        }
    }
}
