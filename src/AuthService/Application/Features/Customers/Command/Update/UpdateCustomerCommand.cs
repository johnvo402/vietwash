using Application.Features.Common.Projections.Accounts;
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

public class UpdateCustomerModel : AccountModel
{
	public string? Email { get; set; }

	public Gender? Gender { get; set; }

	public AccountStatus Status { get; set; }
}
