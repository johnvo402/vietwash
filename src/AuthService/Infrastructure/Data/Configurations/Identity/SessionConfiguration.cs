using Domain.Aggregates.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Identity;

public class SessionConfiguration : IEntityTypeConfiguration<AccountToken>
{
    public void Configure(EntityTypeBuilder<AccountToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Account).WithMany(x => x.AccountTokens).HasForeignKey(x => x.AccountId);
    }
}
