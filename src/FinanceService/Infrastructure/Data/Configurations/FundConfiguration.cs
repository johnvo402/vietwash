using Domain.Aggregates.Funds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class FundBehaviorConfiguration : IEntityTypeConfiguration<Fund>
    {
        public void Configure(EntityTypeBuilder<Fund> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).HasColumnType("citext");
            builder.Property(x => x.Amount).HasColumnType("numeric");
        }
    }
}
