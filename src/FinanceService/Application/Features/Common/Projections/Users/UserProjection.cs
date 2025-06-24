using Application.Common.Security;
using Contracts.Application.Common;
using Domain.Aggregates.Users.Enums;

namespace Application.Features.Common.Projections.Users;

public class UserProjection : BaseResponse
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateOnly? BirthDay { get; set; }

    public Gender? Gender { get; set; }

    public string? Street { get; set; }
    public string? Avatar { get; set; }
    public CustomerGroup? CustomerGroup { get; set; }
    public UserStatus Status { get; set; }
    public string Role { get; set; }
}
