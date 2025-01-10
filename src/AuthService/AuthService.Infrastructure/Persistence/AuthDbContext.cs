
using System.Text;
using AuthService.Domain.Permissions;
using AuthService.Domain.RolePermissions;
using AuthService.Domain.Roles;
using AuthService.Domain.UserActivities;
using AuthService.Domain.UserRoles;
using AuthService.Domain.Users.Entity;
using Micro.Shared.Infrastructure.Policies;
using Micro.Shared.QueryServices;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence;

public class AuthDbContext : DbContext
{
    public DbSet<UserActivity> UserActivities => Set<UserActivity>();

    public DbSet<User> User => Set<User>();
    public DbSet<Role> Role => Set<Role>();
    public DbSet<UserRole> UserRole => Set<UserRole>();
    public DbSet<Permission> Permission => Set<Permission>();
    public DbSet<RolePermission> RolePermission => Set<RolePermission>();


    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);

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
