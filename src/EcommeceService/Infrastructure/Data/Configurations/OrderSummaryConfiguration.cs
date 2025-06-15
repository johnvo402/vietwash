using Domain.Functions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class OrderSummaryConfiguration : IEntityTypeConfiguration<OrderSummaryResult>
    {
        public void Configure(EntityTypeBuilder<OrderSummaryResult> builder)
        {
            builder.HasNoKey();
        }
    }
}
