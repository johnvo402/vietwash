using Contracts.Application.Common.Interfaces.GenIdLong;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts.Infrastructure.Services.GenIdLong
{
    public static class SnowflakeIdGeneratorExtension
    {
        public static IServiceCollection AddSnowflakeIdGenerator(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var machineId = configuration.GetValue<long?>("MachineId");
            if (machineId == null || machineId < 0)
                throw new ArgumentException("Worker ID must be a non-negative number.");

            services.AddSingleton<IIdGenerator>(new SnowflakeIdGenerator((long)machineId));
            return services;
        }
    }
}
