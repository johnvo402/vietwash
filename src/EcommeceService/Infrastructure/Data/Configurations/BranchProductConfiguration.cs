using Domain.Aggregates.Products;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Configurations
{
	public class BranchProductConfiguration : IEntityTypeConfiguration<BranchProduct>
	{
		public void Configure(EntityTypeBuilder<BranchProduct> builder)
		{
			builder.HasKey(x => x.Id);
			builder.HasIndex(x => x.Id);
		}
	}
}
