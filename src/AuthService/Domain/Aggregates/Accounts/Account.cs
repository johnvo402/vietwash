using Ardalis.GuardClauses;
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Events;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Accounts;

public class Account : AggregateRoot
{
    public string DisplayName { get; private set; }
    public string Password { get; private set; }
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
    public AccountStatus Status { get; set; }
    public ICollection<AccountToken>? AccountTokens { get; set; } = [];

    public ICollection<AccountResetPassword>? AccountResetPasswords { get; set; } = [];
    public ICollection<AccountContact>? AccountContacts { get; set; } = [];
    public ICollection<AccountActivity>? AccountActivities { get; set; } = [];

    public ICollection<BranchAccount>? BranchAccounts { get; set; } = [];

    public Account(
        string displayName,
        string password,
        string email,
        string phoneNumber,
        string role,
        string code,
        AccountLanguages language = AccountLanguages.Vi
    )
    {
        DisplayName = Guard.Against.Null(displayName, nameof(DisplayName));
        Password = Guard.Against.Null(password, nameof(Password));
        Email = Guard.Against.Null(email, nameof(Email));
        PhoneNumber = Guard.Against.Null(phoneNumber, nameof(PhoneNumber));
        Role = Guard.Against.NullOrEmpty(role, nameof(Role));
        Code = Guard.Against.NullOrEmpty(code, nameof(Code));
        Language = Guard.Against.Null(language, nameof(Language));
    }

    private Account()
    {
        DisplayName = string.Empty;
        Password = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        Role = string.Empty;
        Code = string.Empty;
    }

    public void SetPassword(string password) =>
        Password = Guard.Against.NullOrWhiteSpace(password, nameof(password));

    public void CreateAccount() => Emit(new AccountCreateEvent() { Account = this });

    protected override bool TryApplyDomainEvent(INotification domainEvent)
    {
        switch (domainEvent)
        {
            case AccountCreateEvent:
                //CreateAccount();
                return true;
            default:
                return false;
        }
    }
}
