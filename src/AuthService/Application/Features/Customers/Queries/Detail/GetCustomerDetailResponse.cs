using Application.Features.Common.Projections.Accounts;
using Domain.Aggregates.Accounts.Enums;

namespace Application.Features.Customers.Queries.Detail;

public class GetCustomerDetailResponse : AccountDetailProjection
{
    public CustomerGroup? CustomerGroup { get; set; }
};
