using System.Collections.Immutable;
using Shared.Kernel.Common.Events;

namespace Domain.Aggregates.Orders.Events;

public sealed record EInvoiceEvent(
    long OrderId,
    string OrderCode,
    DateTimeOffset CompletedAt,
    int Vat,
    decimal VatAmount,
    string CustomerName,
    string? CustomerPhone,
    string? CustomerEmail,
    decimal Total,
    decimal Discount,
    ImmutableArray<EInvoiceOrderItemSnapshot> Items
) : IDomainEvent;

public sealed record EInvoiceOrderItemSnapshot(
    string ServiceName,
    string? UnitRelationName,
    int Quantity,
    decimal UnitPrice
);
