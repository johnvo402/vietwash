using Microsoft.EntityFrameworkCore;

namespace Configurations.Extensions
{
    public static class CheckNullFactoryExtension
    {
        public static void ThrowIfNull<T, TDbContext>(
            this CustomWebApplicationFactory<T, TDbContext>? factory
        )
            where T : class
            where TDbContext : DbContext
        {
            if (factory == null)
            {
                throw new NullReferenceException("factory is null");
            }
        }
    }
}
