using Domain.Aggregates.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Identity;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id);
        builder.Property(x => x.Name).HasColumnType("citext");
        builder.HasIndex(x => x.CategoryId);
        builder.HasOne(x => x.Category).WithMany(c => c.Services).HasForeignKey(x => x.CategoryId);
        builder
            .HasMany(x => x.ServiceTariffs)
            .WithOne(c => c.Service)
            .HasForeignKey(x => x.ServiceId);
    }
}
