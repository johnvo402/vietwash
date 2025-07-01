using Application.Common.Interfaces.UnitOfWorks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Contracts.Extensions
{
    public static class MigrationExtensions
    {
        public static IApplicationBuilder ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();

            try
            {
                dbContext.DatabaseFacade.Migrate();
            }
            catch (Exception ex)
            {
                // Có thể log hoặc throw lại tùy theo bạn muốn ứng dụng tiếp tục hay dừng
                var logger = scope
                    .ServiceProvider.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Migration");
                logger.LogError(ex, "An error occurred while migrating the database.");
                throw;
            }

            return app;
        }
    }
}
