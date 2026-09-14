using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Events;
using Shared.Kernel.Common;

namespace Application.Common.DomainEventHandlers
{
    internal class CreateAccountEvent : BaseEntity
    {
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? Code { get; set; }
        public string? PhoneNumber { get; set; }
        public DateOnly BirthDay { get; set; }
        public Gender? Gender { get; set; }
        public string? AvtUrl { get; set; }
        public string? Role { get; set; }
        public bool Disabled { get; set; }
        public CustomerGroup? CustomerGroup { get; set; }

        public AccountStatus Status { get; set; }

        public virtual void MappingFrom(AccountCreateEvent account)
        {
            Id = account.AccountId;
            PublicId = account.PublicId;
            CreatedAt = account.CreatedAt;
            CreatedBy = account.CreatedBy;
            UpdatedAt = account.UpdatedAt;
            UpdatedBy = account.UpdatedBy;

            DisplayName = account.DisplayName;
            Code = account.Code;
            Email = account.Email;
            PhoneNumber = account.PhoneNumber;
            AvtUrl = account.AvtUrl;
            Role = account.Role;
            Status = account.Status;
            CustomerGroup = account.CustomerGroup;
            BirthDay = account.BirthDay;
            Disabled = account.Disabled;
            Gender = account.Gender;
        }
    }
}
