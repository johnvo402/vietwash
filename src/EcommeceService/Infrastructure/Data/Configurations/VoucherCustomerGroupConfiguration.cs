using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Vouchers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class VoucherCustomerGroupConfiguration : IEntityTypeConfiguration<VoucherCustomerGroup>
    {
        public void Configure(EntityTypeBuilder<VoucherCustomerGroup> builder)
        {
            builder.HasKey(x => x.Id);

            builder
                .HasOne(x => x.Voucher)
                .WithMany(x => x.VoucherCustomerGroups)
                .HasForeignKey(x => x.VoucherId);

            builder.HasIndex(x => new { x.VoucherId, x.Group }).IsUnique();

            builder.Property(x => x.CreatedAt).IsRequired();
        }
    }
}
