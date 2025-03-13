using Domain.Aggregates.Funds;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Data.Configurations
{
    public class FundTypeConfiguration : IEntityTypeConfiguration<FundType>
    {
        public void Configure(EntityTypeBuilder<FundType> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.Property(x => x.Name).HasColumnType("citext");
        }
    }
}
