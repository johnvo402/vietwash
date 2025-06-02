using Application.Feature.Common.Projections.Services;
using Contracts.Routers;
using Domain.Aggregates.Services.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Services.Command.Update;

public class UpdateServiceCommand : IRequest<UpdateServiceResponse>
{
    [FromRoute(Name = RouterBase.Id)]
    public long ServiceId { get; set; } = default!;
	[FromBody]
    public ServiceModel Service { get; set; } = default!;
}
