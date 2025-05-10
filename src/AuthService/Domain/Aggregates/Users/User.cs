using Ardalis.GuardClauses;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Events;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Users;

public class User : AggregateRoot
{
    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string Username { get; private set; }

    public string Password { get; private set; }

    public string Email { get; private set; }

    public string PhoneNumber { get; set; }

    public DateTime? DayOfBirth { get; set; }

    public Gender? Gender { get; set; }

    public string? Avatar { get; set; }

    public string Role { get; set; } = string.Empty;

    public UserStatus Status { get; set; } = UserStatus.Active;
    public ICollection<UserToken>? UserTokens { get; set; } = [];

    public ICollection<UserResetPassword>? UserResetPasswords { get; set; } = [];


    public User(
        string firstName,
        string lastName,
        string username,
        string password,
        string email,
        string phoneNumber,
        string role
    )
    {
        FirstName = Guard.Against.NullOrEmpty(firstName, nameof(FirstName));
        LastName = Guard.Against.Null(lastName, nameof(LastName));
        Username = Guard.Against.Null(username, nameof(Username));
        Password = Guard.Against.Null(password, nameof(Password));
        Email = Guard.Against.Null(email, nameof(Email));
        PhoneNumber = Guard.Against.Null(phoneNumber, nameof(PhoneNumber));
        Role = Guard.Against.NullOrEmpty(role, nameof(Role));
        
    }

    private User()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Username = string.Empty;
        Password = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        Role = string.Empty;
    }

    public void SetPassword(string password) =>
        Password = Guard.Against.NullOrWhiteSpace(password, nameof(password));

    public void CreateUser() =>
        Emit(new UserCreateEvent() { User = this });

    protected override bool TryApplyDomainEvent(INotification domainEvent)
    {
        switch (domainEvent)
        {
            case UserCreateEvent:
                //CreateUser();
                return true;
            default:
                return false;
        }
    }
}
