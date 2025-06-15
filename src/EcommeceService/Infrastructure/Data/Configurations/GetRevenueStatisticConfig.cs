using Domain.Functions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class GetRevenueStatisticConfiguration : IEntityTypeConfiguration<GetRevenueStatistic>
{
    public void Configure(EntityTypeBuilder<GetRevenueStatistic> builder)
    {
        builder.HasNoKey();
    }
}
