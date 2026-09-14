using Domain.Aggregates.Accounts.Enums;
using Shared.Kernel.Common.Events;

namespace Domain.Aggregates.Accounts.Events;

public sealed record AccountCreateEvent(
    long AccountId,
    Ulid PublicId,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy,
    string DisplayName,
    string? Email,
    string Code,
    string PhoneNumber,
    DateOnly BirthDay,
    Gender? Gender,
    string? AvtUrl,
    string Role,
    bool Disabled,
    CustomerGroup? CustomerGroup,
    AccountStatus Status
) : IDomainEvent;
