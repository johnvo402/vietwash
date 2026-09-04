using System.Text.Json;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
        if (order.Status != OrderStatus.Processed || order.CustomerId is not long customerId
            || !order.UncommittedEvents.OfType<UpdateStatusOrderEvent>().Any())
            return null;
        var now = DateTimeOffset.UtcNow;
        return new NotificationOutbox
        {
            Id = $"order-processed:{order.Id}",
            CreatedAt = now,
            NextAttemptAt = now,
            Payload = JsonSerializer.Serialize(new ProcessedOrderNotification(
                order.Id, order.PublicId.ToString(), order.Code, order.BranchId, customerId, now)),
        };
    }
}

public sealed record ProcessedOrderNotification(long OrderId, string PublicId, string OrderCode,
    long BranchId, long CustomerId, DateTimeOffset OccurredAt);

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
