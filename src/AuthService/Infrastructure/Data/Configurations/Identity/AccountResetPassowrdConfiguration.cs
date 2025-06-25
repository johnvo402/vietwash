using Domain.Aggregates.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Identity;

public class AccountResetPassowrdConfiguration : IEntityTypeConfiguration<AccountResetPassword>
{
    public void Configure(EntityTypeBuilder<AccountResetPassword> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.Account)
            .WithMany(x => x.AccountResetPasswords)
            .HasForeignKey(x => x.AccountId);
    }
}
