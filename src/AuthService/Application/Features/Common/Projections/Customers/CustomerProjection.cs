using Application.Common.Security;
using Contracts.Application.Common;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;

namespace Application.Features.Common.Projections.Customers
{
    public class CustomerProjection : BaseResponse
    {
        public string? DisplayName { get; set; }
        public string? PhoneNumber { get; set; }
        public CustomerGroup? CustomerGroup { get; set; }

        [File]
        public string? AvtUrl { get; set; }
        public AccountStatus Status { get; set; }

        public virtual void MappingFrom(Account account)
        {
            Id = account.Id;
            PublicId = account.PublicId;
            CreatedAt = account.CreatedAt;
            CreatedBy = account.CreatedBy;
            UpdatedAt = account.UpdatedAt;
            UpdatedBy = account.UpdatedBy;

            DisplayName = account.DisplayName;
            PhoneNumber = account.PhoneNumber;
            AvtUrl = account.AvtUrl;
            Status = account.Status;
        }
    }
}
