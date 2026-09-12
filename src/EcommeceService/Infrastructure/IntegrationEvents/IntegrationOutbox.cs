using System.Text.Json;
using Application.Common.HandleEventDomains.Orders;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Events;
using Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.IntegrationEvents;

public sealed class IntegrationOutbox
{
    public const string CreateFundTopic = "CreateFundEvent";
    public const string EInvoiceTopic = "EInvoiceEvent";

    public string Id { get; set; } = null!;
    public string Topic { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset NextAttemptAt { get; set; }
    public int Attempts { get; set; }
    public Guid? LeaseId { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public string? LastError { get; set; }

    public static IReadOnlyList<IntegrationOutbox> FromOrder(Order order)
    {
        var messages = new List<IntegrationOutbox>();
        foreach (var domainEvent in order.UncommittedEvents)
        {
            switch (domainEvent)
            {
                case CreateFundEvent fund:
                    string fundId = $"order-completed:{order.Id}:finance";
                    fund.MessageId = Guid.NewGuid();
                    messages.Add(Create(fundId, CreateFundTopic, fund));
                    break;
                case EInvoiceEvent:
                    string invoiceId = $"order-completed:{order.Id}:einvoice";
                    EInvoiceOrderMessage invoice = order.ToEInvoiceMessage();
                    invoice.MessageId = Guid.NewGuid();
                    messages.Add(Create(invoiceId, EInvoiceTopic, invoice));
                    break;
            }
        }

        return messages;
    }

    private static IntegrationOutbox Create<T>(string id, string topic, T payload)
    {
        var now = DateTimeOffset.UtcNow;
        return new IntegrationOutbox
        {
            Id = id,
            Topic = topic,
            Payload = JsonSerializer.Serialize(payload),
            CreatedAt = now,
            NextAttemptAt = now,
        };
    }
}

public sealed class IntegrationOutboxConfiguration : IEntityTypeConfiguration<IntegrationOutbox>
{
    public void Configure(EntityTypeBuilder<IntegrationOutbox> builder)
    {
        builder.ToTable("integration_outbox");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(128);
        builder.Property(x => x.Topic).HasMaxLength(128);
        builder.Property(x => x.Payload).HasColumnType("jsonb");
        builder.Property(x => x.LastError).HasMaxLength(256);
        builder
            .HasIndex(x => new { x.NextAttemptAt, x.LockedUntil })
            .HasFilter("delivered_at IS NULL");
    }
}
