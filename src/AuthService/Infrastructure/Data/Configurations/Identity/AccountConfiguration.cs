using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Identity;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id);
        builder.Property(x => x.BirthDay).HasColumnType("date");
        builder.Property(x => x.Email).HasColumnType("citext");
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.PhoneNumber).IsUnique();
        builder.Property(x => x.Status).HasDefaultValue(AccountStatus.Active);
        builder.Property(x => x.PhoneCode).HasDefaultValue("+84");
        builder.Property(x => x.Disabled).HasDefaultValue(false);
        builder.Property(x => x.Verified).HasDefaultValue(false);
        builder
            .HasMany(x => x.BranchAccounts)
            .WithOne()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
