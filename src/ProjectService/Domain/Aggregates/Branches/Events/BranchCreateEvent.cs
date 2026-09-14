using Shared.Kernel.Common.Events;

namespace Domain.Aggregates.Branches.Events
{
    public sealed record BranchCreateEvent(long BranchId, string Name) : IDomainEvent;
}
