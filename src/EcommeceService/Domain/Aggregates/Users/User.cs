using Ardalis.GuardClauses;
using Domain.Aggregates.Users.Enums; 
using Domain.Aggregates.Users.ValueObjects;
using JohnChum.SharedKernel.Domain.Common; 

namespace Domain.Aggregates.Users;

public class User : BaseEntity
{
    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string Username { get; private set; }

    public string Email { get; private set; }

    public string PhoneNumber { get; set; }

    public DateTime? DayOfBirth { get; set; }

    public Gender? Gender { get; set; }

    public Address? Address { get; private set; }

    public string? Avatar { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;
    public Ulid RoleId { get; set; }





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

        Email = string.Empty;
        PhoneNumber = string.Empty;
        RoleId = new Ulid();
    }



    public void UpdateAddress(Address address) => Address = address;

}
