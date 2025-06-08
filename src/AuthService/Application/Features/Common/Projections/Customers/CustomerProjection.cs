
using Application.Common.Security;
using Domain.Aggregates.Accounts.Enums;
using JohnChum.SharedKernel.Application.Common;
using Domain.Aggregates.Accounts;

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

       
    }
}
