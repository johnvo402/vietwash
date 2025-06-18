using Domain.Aggregates.PubSubLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class DeadLetterConfiguration : IEntityTypeConfiguration<PubSubLog>
{
    public void Configure(EntityTypeBuilder<PubSubLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ErrorDetail).HasColumnType("jsonb");
        builder.Property(x => x.Request).HasColumnType("jsonb");
    }
}
