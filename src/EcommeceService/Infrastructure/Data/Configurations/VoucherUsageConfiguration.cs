using Domain.Aggregates.Orders;
using Domain.Aggregates.Users;
using Domain.Aggregates.Vouchers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class VoucherUsageConfiguration : IEntityTypeConfiguration<VoucherUsage>
    {
        public void Configure(EntityTypeBuilder<VoucherUsage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DiscountApply).HasColumnType("numeric").IsRequired();

            builder.HasOne<Voucher>().WithMany().HasForeignKey(x => x.VoucherId);

            builder.HasOne<User>().WithMany().HasForeignKey(x => x.CustomerId);

            builder
                .HasOne(x => x.Order)
                .WithOne(o => o.VoucherUsage)
                .HasForeignKey<VoucherUsage>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
