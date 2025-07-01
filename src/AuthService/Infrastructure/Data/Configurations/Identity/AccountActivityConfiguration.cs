using Domain.Aggregates.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Identity
{
    public class AccountActivityConfiguration : IEntityTypeConfiguration<AccountActivity>
    {
        public void Configure(EntityTypeBuilder<AccountActivity> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder
                .HasOne(x => x.Account)
                .WithMany(x => x.AccountActivities)
                .HasForeignKey(x => x.AccountId);
        }
    }
}
