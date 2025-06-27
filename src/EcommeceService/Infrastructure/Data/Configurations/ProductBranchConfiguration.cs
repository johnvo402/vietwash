using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Products;

namespace Infrastructure.Data.Configurations
{

	public class ProductBranchConfiguration : IEntityTypeConfiguration<ProductBranch>
	{
		public void Configure(EntityTypeBuilder<ProductBranch> builder)
		{
			builder.HasKey(x => x.Id);
			builder.HasIndex(x => x.Id);
			builder.HasOne(x => x.Product).WithMany(x => x.ProductBranches).HasForeignKey(x => x.ProductId);

		}
	}
}
