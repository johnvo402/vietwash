using Domain.Aggregates.Funds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class FundBehaviorConfiguration : IEntityTypeConfiguration<FundBehavior>
    {
        public void Configure(EntityTypeBuilder<FundBehavior> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.Property(x => x.Name).HasColumnType("citext");
        }
    }
}
