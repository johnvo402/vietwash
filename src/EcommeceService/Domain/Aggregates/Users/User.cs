using Ardalis.GuardClauses;
using Domain.Aggregates.Users.Enums;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Users;

public class User : BaseEntity
{
    public string DisplayName { get; private set; }
    public string? Email { get; private set; }
    public string Code { get; private set; }
    public string PhoneNumber { get; private set; }
    public DateOnly? BirthDay { get; set; }
    public Gender? Gender { get; set; }
    public string? AvtUrl { get; set; }
    public string Role { get; private set; }
    public bool Disabled { get; set; }
    public CustomerGroup? CustomerGroup { get; set; }
    public UserStatus Status { get; set; }
    public ICollection<BranchUser>? BranchUsers { get; set; } = [];

    public User(string displayName, string email, string phoneNumber, string role, string code)
    {
        DisplayName = Guard.Against.Null(displayName, nameof(DisplayName));
        Email = Guard.Against.Null(email, nameof(Email));
        PhoneNumber = Guard.Against.Null(phoneNumber, nameof(PhoneNumber));
        Role = Guard.Against.NullOrEmpty(role, nameof(Role));
        Code = Guard.Against.NullOrEmpty(code, nameof(Code));
    }

    public void Update(
        string? displayName = null,
        string? email = null,
        string? phoneNumber = null,
        DateOnly? birthDay = null,
        Gender? gender = null,
        string? role = null,
        UserStatus? status = null
    )
    {
        if (!string.IsNullOrWhiteSpace(displayName))
            DisplayName = displayName.Trim();

        if (!string.IsNullOrWhiteSpace(email))
            Email = email.Trim();

        if (!string.IsNullOrWhiteSpace(phoneNumber))
            PhoneNumber = phoneNumber.Trim();

        if (birthDay.HasValue)
            BirthDay = birthDay.Value;

        if (gender.HasValue)
            Gender = gender;

        if (!string.IsNullOrWhiteSpace(role))
            Role = role.Trim();

        if (status.HasValue)
            Status = status.Value;
    }

    private User()
    {
        DisplayName = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        Role = string.Empty;
        Code = string.Empty;
    }
}
