using Ardalis.GuardClauses;
using Domain.Aggregates.Users.Enums;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Users;

public class User : BaseEntity
{
    public string DisplayName { get; private set; }
    public string Email { get; private set; }
    public string Code { get; private set; }
    public string PhoneNumber { get; private set; }
    public DateOnly BirthDay { get; set; }
    public Gender? Gender { get; set; }
    public string? AvtUrl { get; set; }
    public bool PhoneEnabled { get; private set; }
    public bool EmailEnabled { get; private set; }
    public string Role { get; private set; }
    public AccountLanguages Language { get; private set; }
    public bool Disabled { get; set; }
    public UserStatus Status { get; set; }





    public User(
           string displayName,
           string email,
           string phoneNumber,
           string role,
           string code,
           AccountLanguages language = AccountLanguages.Vi
       )
    {
        DisplayName = Guard.Against.Null(displayName, nameof(DisplayName));
        Email = Guard.Against.Null(email, nameof(Email));
        PhoneNumber = Guard.Against.Null(phoneNumber, nameof(PhoneNumber));
        Role = Guard.Against.NullOrEmpty(role, nameof(Role));
        Code = Guard.Against.NullOrEmpty(code, nameof(Code));
        Language = Guard.Against.Null(language, nameof(Language));
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
