using Domain.Aggregates.Funds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class FundConfiguration : IEntityTypeConfiguration<FundBehavior>
    {
        public void Configure(EntityTypeBuilder<FundBehavior> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
