using System.ComponentModel.DataAnnotations.Schema;
using Ardalis.GuardClauses;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Events;
using Domain.Aggregates.Users.ValueObjects;
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

    public Address? Address { get; private set; }

    public string? Avatar { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;
    public Ulid RoleId { get; set; }
    public ICollection<UserToken>? UserTokens { get; set; } = [];

    public ICollection<UserResetPassword>? UserResetPasswords { get; set; } = [];

    public Role Role { get; set; } = default!;


    public User(
        string firstName,
        string lastName,
        string username,
        string password,
        string email,
        string phoneNumber,
        Ulid roleId,
        Address? address = null
    )
    {
        FirstName = Guard.Against.NullOrEmpty(firstName, nameof(FirstName));
        LastName = Guard.Against.Null(lastName, nameof(LastName));
        Username = Guard.Against.Null(username, nameof(Username));
        Password = Guard.Against.Null(password, nameof(Password));
        Email = Guard.Against.Null(email, nameof(Email));
        PhoneNumber = Guard.Against.Null(phoneNumber, nameof(PhoneNumber));
        Address = address;
        RoleId = roleId;
    }

    private User()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Username = string.Empty;
        Password = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        RoleId = new Ulid();
    }

    public void SetPassword(string password) =>
        Password = Guard.Against.NullOrWhiteSpace(password, nameof(password));

    public void UpdateAddress(Address address) => Address = address;

    public void UpdateDefaultUserClaims() =>
        Emit(new UpdateDefaultUserClaimEvent() { User = this });

    protected override bool TryApplyDomainEvent(INotification domainEvent)
    {
        switch (domainEvent)
        {
            case UpdateDefaultUserClaimEvent:
                //ApplyUpdateDefaultUserClaim();
                return true;
            default:
                return false;
        }
    }
}
