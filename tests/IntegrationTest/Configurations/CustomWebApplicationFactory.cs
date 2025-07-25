using System.Data.Common;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Configurations
{
    public class CustomWebApplicationFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>
        where TProgram : class
        where TDbContext : DbContext
    {
        private readonly DbConnection _dbConnection;
        private readonly string _environmentName;

        public CustomWebApplicationFactory(DbConnection dbConnection, string environmentName)
        {
            _dbConnection = dbConnection;
            _environmentName = environmentName;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove existing DbContextOptions and DbConnection
                var dbContextDescriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<TDbContext>)
                );
                if (dbContextDescriptor != null)
                    services.Remove(dbContextDescriptor);

                var dbConnectionDescriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbConnection)
                );
                if (dbConnectionDescriptor != null)
                    services.Remove(dbConnectionDescriptor);

                // Register the generic DbContext
                services.AddDbContext<TDbContext>(
                    (container, options) => options.UseNpgsql(_dbConnection)
                );

                // Mock ICurrentUser
                services
                    .RemoveAll<ICurrentAccount>()
                    .AddTransient(provider =>
                        Mock.Of<ICurrentAccount>(x =>
                            x.Id == TestingFixture<TProgram, TDbContext>.GetAccountId()
                        )
                    );
            });

            builder.UseEnvironment(_environmentName);
        }
    }
}
