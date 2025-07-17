    using Domain.Aggregates.Vouchers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class VoucherCustomerConfiguration : IEntityTypeConfiguration<VoucherCustomer>
{
    public void Configure(EntityTypeBuilder<VoucherCustomer> builder)
    {
        builder.HasKey(x => x.Id);
        builder
            .HasOne(x => x.Voucher)
            .WithMany(x => x.VoucherCustomers)
            .HasForeignKey(x => x.VoucherId);

        builder.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
    }
}
