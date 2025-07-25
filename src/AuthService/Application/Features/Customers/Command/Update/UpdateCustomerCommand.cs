using Application.Features.Common.Projections.Customers;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Domain.Aggregates.Accounts.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Customers.Command.Update;

public class UpdateCustomerCommand : IRequest<Result>
{
    [FromRoute(Name = RouterBase.Id)]
    public long AccountId { get; set; }

    [FromBody]
    public UpdateCustomerModel? Account { get; set; }
}

public class UpdateCustomerModel : CustomerModel
{
    public AccountStatus Status { get; set; }
}
