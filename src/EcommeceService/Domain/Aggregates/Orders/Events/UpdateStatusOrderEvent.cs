using Domain.Aggregates.Orders.Enums;
using Shared.Kernel.Common.Events;

namespace Domain.Aggregates.Orders.Events;

public sealed record UpdateStatusOrderEvent(
    long OrderId,
    OrderStatus Status,
    string OrderCode,
    string PublicId,
    long BranchId,
    long? CustomerId
) : IDomainEvent;
