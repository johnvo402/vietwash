using Domain.Aggregates.Funds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class FundConfiguration : IEntityTypeConfiguration<Fund>
    {
        public void Configure(EntityTypeBuilder<Fund> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.Property(x => x.Name).HasColumnType("citext");
            builder.Property(x => x.Amount).HasColumnType("numeric");
            builder.HasOne(x => x.FundType).WithMany(x => x.Funds).HasForeignKey(x => x.TypeId);
            builder.HasOne(x => x.FundBehavior).WithMany(x => x.Funds).HasForeignKey(x => x.BehaviorId);

        }
    }
}
