using Contracts.ApiWrapper;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Users.Enums;
using Mediator;

namespace Application.Features.Users.Queries.Detail;

public sealed record GetCustomerQuery(long Id) : IRequest<Result<GetCustomerResponse>>;

public sealed class GetCustomerResponse
{
    public long Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public CustomerGroup? CustomerGroup { get; init; }
    public ActivationStatus Status { get; init; }
    public string Role { get; init; } = string.Empty;
}
