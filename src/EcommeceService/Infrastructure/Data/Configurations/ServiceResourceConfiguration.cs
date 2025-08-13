using Domain.Aggregates.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Identity;

public class ServiceResourceConfiguration : IEntityTypeConfiguration<ServiceResource>
{
    public void Configure(EntityTypeBuilder<ServiceResource> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id);

        builder
            .HasOne(x => x.UnitRelation)
            .WithMany(x => x.AsUnitRelation)
            .HasForeignKey(x => x.UnitRelationId);
        builder
            .HasOne(x => x.UnitProduct)
            .WithMany(x => x.AsUnitProduct)
            .HasForeignKey(x => x.UnitProductId);
        builder.HasOne(x => x.BranchProduct).WithMany().HasForeignKey(x => x.ProductId);
    }
}
