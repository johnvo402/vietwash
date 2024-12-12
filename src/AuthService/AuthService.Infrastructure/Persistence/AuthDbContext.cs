using AuthService.Domain.Entities;
using Micro.Shared.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence;

public class AuthDbContext : IdentityDbContext<User, Role, string>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (tableName.StartsWith("AspNet"))
            {
                entityType.SetTableName(tableName.Substring(6));
            }
        }
        // var rolenames = typeof(RoleName).GetFields().ToList();
        // foreach (var r in rolenames)
        // {
        //     if (r.Name == "DefaultRole") continue;
        //     var rolename = (string)r.GetRawConstantValue();

        //     IdentityRole role = new IdentityRole(rolename);
        //     modelBuilder.Entity<IdentityRole>().HasData(role);
        // }

    }

}
