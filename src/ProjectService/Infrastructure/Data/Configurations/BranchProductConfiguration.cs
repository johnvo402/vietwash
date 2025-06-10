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
    public class BranchProductConfiguration : IEntityTypeConfiguration<BranchProduct>
    {
        public void Configure(EntityTypeBuilder<BranchProduct> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.HasOne(x => x.Branch).WithMany(x => x.BranchProducts).HasForeignKey(x => x.BranchId);

        }
    }
}
