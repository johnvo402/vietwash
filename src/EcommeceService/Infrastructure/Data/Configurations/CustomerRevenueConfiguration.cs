using Domain.Functions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class CustomerRevenueConfiguration : IEntityTypeConfiguration<CustomerRevenueResult>
    {
        public void Configure(EntityTypeBuilder<CustomerRevenueResult> builder)
        {
            builder.HasNoKey();
            builder.ToView(null);
        }
    }
}
