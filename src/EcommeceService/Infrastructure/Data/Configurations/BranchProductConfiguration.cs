using Domain.Aggregates.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class BranchProductConfiguration : IEntityTypeConfiguration<BranchProduct>
    {
        public void Configure(EntityTypeBuilder<BranchProduct> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.HasMany(x => x.ProductSupplyings).WithOne().HasForeignKey(x => x.ProductId);
            builder.HasMany(x => x.UnitRelations).WithOne().HasForeignKey(x => x.BranchProductId);
        }
    }
}
