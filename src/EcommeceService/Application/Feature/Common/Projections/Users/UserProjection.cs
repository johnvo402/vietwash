using Application.Common.Security;
using Contracts.Application.Common;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;

namespace Application.Features.Common.Projections.Users;

public class UserProjection : BaseResponse
{
    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateOnly? BirthDay { get; set; }

    public Gender? Gender { get; set; }

    [File]
    public string? Avatar { get; set; }
    public CustomerGroup? CustomerGroup { get; set; }
    public ActivationStatus Status { get; set; }
    public string Role { get; set; } = default!;

    public virtual void MappingFrom(User user)
    {
        Id = user.Id;
        PublicId = user.PublicId;
        CreatedAt = user.CreatedAt;
        CreatedBy = user.CreatedBy;
        UpdatedAt = user.UpdatedAt;
        UpdatedBy = user.UpdatedBy;

        DisplayName = user.DisplayName;
        Email = user.Email;
        PhoneNumber = user.PhoneNumber;
        BirthDay = user.BirthDay;
        Gender = user.Gender;
        Avatar = user.AvtUrl;
        CustomerGroup = user.CustomerGroup;
        Status = user.Status;
        Role = user.Role;
    }
}

public class UserDTO
{
    public long? Id { get; set; }
    public Ulid? PublicId { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public CustomerGroup? CustomerGroup { get; set; }

    [File]
    public string? Avatar { get; set; }

    public virtual void MappingFrom(User user)
    {
        Id = user.Id;
        PublicId = user.PublicId;
        DisplayName = user.DisplayName;
        Email = user.Email;
        PhoneNumber = user.PhoneNumber;
        CustomerGroup = user.CustomerGroup;
        Avatar = user.AvtUrl;
    }
}
