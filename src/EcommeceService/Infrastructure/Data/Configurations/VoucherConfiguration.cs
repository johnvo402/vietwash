using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Vouchers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Identity;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DiscountValue).HasColumnType("numeric");
        builder
            .Property(x => x.CustomerGroups)
            .HasColumnType("smallint[]")
            .HasConversion(
                v => v.Select(e => (short)e).ToArray(),
                v =>
                    v == null ? new List<CustomerGroup>() : v.Select(e => (CustomerGroup)e).ToList()
            );
    }
}
