using Domain.Aggregates.Inventories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class ProductSupplyingConfiguration : IEntityTypeConfiguration<ProductSupplying>
    {
        public void Configure(EntityTypeBuilder<ProductSupplying> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder
                .HasOne(x => x.UnitRelation)
                .WithMany(x => x.ProductSupplyings)
                .HasForeignKey(x => x.UnitRelationId);
            builder
                .HasOne(x => x.Supplier)
                .WithMany(x => x.ProductSupplyings)
                .HasForeignKey(x => x.SupplierId);
        }
    }
}
