using Domain.Aggregates.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Identity
{
    public class BranchAccountConfiguration : IEntityTypeConfiguration<BranchAccount>
    {
        public void Configure(EntityTypeBuilder<BranchAccount> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
        }
    }
}
