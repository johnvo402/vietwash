using Ardalis.GuardClauses;
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Events;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Accounts;

public class Account : AggregateRoot
{
    public string DisplayName { get; private set; }
    public string? Password { get; private set; }
    public string? Email { get; private set; }
    public string Code { get; set; }
    public string PhoneNumber { get; private set; }
    public string PhoneCode { get; private set; }
    public DateOnly BirthDay { get; set; }
    public Gender? Gender { get; set; }
    public string? AvtUrl { get; set; }
    public bool Verified { get; private set; }
    public string Role { get; set; }
    public CustomerGroup? CustomerGroup { get; set; }
    public bool Disabled { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public ICollection<AccountToken>? AccountTokens { get; set; } = [];

    public ICollection<AccountResetPassword>? AccountResetPasswords { get; set; } = [];
    public AccountContact? AccountContact { get; set; }
    public ICollection<AccountActivity>? AccountActivities { get; set; } = [];

    public ICollection<BranchAccount>? BranchAccounts { get; set; } = [];

    public Account(
        string? displayName,
        string? password,
        string? email,
        string phoneNumber,
        string role,
        string code,
        string phoneCode = "+84"
    )
    {
        DisplayName = Guard.Against.Null(displayName, nameof(DisplayName));
        Password = password;
        Email = email;
        PhoneNumber = Guard.Against.Null(phoneNumber, nameof(PhoneNumber));
        Role = Guard.Against.NullOrEmpty(role, nameof(Role));
        Code = Guard.Against.NullOrEmpty(code, nameof(Code));
        PhoneCode = phoneCode;
        Status = AccountStatus.Active;
    }

    public void Update(
        string? displayName = null,
        string? email = null,
        string? phoneNumber = null,
        DateOnly? birthDay = null,
        Gender? gender = null,
        string? role = null,
        AccountStatus? status = null
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

    private Account()
    {
        DisplayName = string.Empty;
        Password = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        Role = string.Empty;
        Code = string.Empty;
        PhoneCode = string.Empty;
    }

    public void SetPassword(string password) =>
        Password = Guard.Against.NullOrWhiteSpace(password, nameof(password));

    public void CreateAccount() => Emit(new AccountCreateEvent() { Account = this });

    public void VerifiedCustomer() => this.Verified = true;

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
