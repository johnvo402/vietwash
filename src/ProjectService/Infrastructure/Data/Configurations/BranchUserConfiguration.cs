using Domain.Aggregates.Branches;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configurations
{
	public class BranchUserConfiguration : IEntityTypeConfiguration<BranchUser>
	{
		public void Configure(EntityTypeBuilder<BranchUser> builder)
		{
			builder.HasKey(x => x.Id);
			builder.HasIndex(x => x.Id);
			builder.HasOne(x => x.Branch).WithMany(x => x.BranchUsers).HasForeignKey(x => x.BranchId);

		}
	}
}
