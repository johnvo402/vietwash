using System.Text.Json;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Kernel.Common.Events;

namespace Infrastructure.Notifications;

// An immutable integration intent, committed in the SAME transaction as Order.
public sealed class NotificationOutbox
{
    public string Id { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset NextAttemptAt { get; set; }
    public int Attempts { get; set; }
    public Guid? LeaseId { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public string? LastError { get; set; }

    public static NotificationOutbox? FromOrder(Order order)
    {
        UpdateStatusOrderEvent? transition = order.UncommittedEvents
            .OfType<UpdateStatusOrderEvent>()
            .LastOrDefault(x => x.Status == OrderStatus.Processed);
        if (order.Status != OrderStatus.Processed || transition?.CustomerId is not long customerId)
            return null;
        var now = DateTimeOffset.UtcNow;
        return new NotificationOutbox
        {
            Id = $"order-processed:{transition.OrderId}",
            CreatedAt = now,
            NextAttemptAt = now,
            Payload = JsonSerializer.Serialize(new ProcessedOrderNotification(
                transition.OrderId, transition.PublicId, transition.OrderCode,
                transition.BranchId, customerId, now)),
        };
    }
}

public sealed record ProcessedOrderNotification(long OrderId, string PublicId, string OrderCode,
    long BranchId, long CustomerId, DateTimeOffset OccurredAt) : IIntegrationEvent;

public sealed class NotificationOutboxConfiguration : IEntityTypeConfiguration<NotificationOutbox>
{
    public void Configure(EntityTypeBuilder<NotificationOutbox> builder)
    {
        builder.ToTable("notification_outbox");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(128);
        builder.Property(x => x.Payload).HasColumnType("jsonb");
        builder.Property(x => x.LastError).HasMaxLength(256);
        builder.HasIndex(x => new { x.NextAttemptAt, x.LockedUntil }).HasFilter("delivered_at IS NULL");
    }
}
