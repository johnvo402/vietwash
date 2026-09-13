using Domain.Aggregates.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class PayOsCancellationRequestConfiguration
    : IEntityTypeConfiguration<PayOsCancellationRequest>
{
    public void Configure(EntityTypeBuilder<PayOsCancellationRequest> builder)
    {
        builder.ToTable("payos_cancellation_request");
        builder.HasKey(x => x.OrderId);
        builder.Property(x => x.OrderId).ValueGeneratedNever();
        builder.Property(x => x.State).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Reason).HasMaxLength(OrderCancellation.MaximumReasonLength);
        builder.Property(x => x.ProviderState).HasMaxLength(32);
        builder.Property(x => x.LastError).HasMaxLength(256);
        builder.HasIndex(x => new { x.State, x.NextAttemptAt, x.LockedUntil });
    }
}
