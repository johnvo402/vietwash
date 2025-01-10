using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Roles;
using AuthService.Domain.ValueObjects;


namespace AuthService.Infrastructure.Configuration
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            // Cấu hình thuộc tính OrgId
            builder.Property(r => r.OrgId)
                .HasColumnName("org_id")
                .HasDefaultValue("DOAN");
            builder.HasIndex(r => r.Id).HasDatabaseName("ix_role_id");
        }

    }
}
