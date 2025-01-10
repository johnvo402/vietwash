using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Roles;
using AuthService.Domain.Permissions;


namespace AuthService.Infrastructure.Configuration
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasIndex(r => r.Id).HasDatabaseName("ix_permission_id");
        }
        
    }
}
