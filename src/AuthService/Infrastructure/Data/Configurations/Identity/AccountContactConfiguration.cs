using Domain.Aggregates.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Infrastructure.Data.Configurations.Identity
{
    public class AccountContactConfiguration : IEntityTypeConfiguration<AccountContact>
    {
        public void Configure(EntityTypeBuilder<AccountContact> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.HasOne(x => x.Account)
                .WithMany(x => x.AccountContacts)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
