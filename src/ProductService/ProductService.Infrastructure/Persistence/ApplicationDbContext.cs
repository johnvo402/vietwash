using Micro.Shared.QueryServices;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Product => Set<Product>();
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
       : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (tableName is not null)
            {
                entity.SetTableName(DapperQueryBuilder.ToSnakeCase(tableName));
            }

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(DapperQueryBuilder.ToSnakeCase(property.Name));
            }

            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (keyName is not null)
                {
                    key.SetName(DapperQueryBuilder.ToSnakeCase(keyName));
                }
            }

            foreach (var index in entity.GetIndexes())
            {
                if (index.Name is not null)
                {
                    index.SetDatabaseName(DapperQueryBuilder.ToSnakeCase(index.Name));
                }
            }
        }

    }
}