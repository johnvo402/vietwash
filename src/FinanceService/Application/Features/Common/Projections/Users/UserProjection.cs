using Contracts.Application.Common;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Users;
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
    public ActivationStatus Status { get; set; }
    public string Role { get; set; }
}

public class UserDTO
{
    public long? Id { get; set; }
    public string? DisplayName { get; set; }

    public Ulid? PublicId { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public CustomerGroup? CustomerGroup { get; set; }

    public virtual void MappingFrom(User user)
    {
        Id = user.Id;
        PublicId = user.PublicId;
        DisplayName = user.DisplayName;
        Email = user.Email;
        PhoneNumber = user.PhoneNumber;
        CustomerGroup = user.CustomerGroup;
    }
}
