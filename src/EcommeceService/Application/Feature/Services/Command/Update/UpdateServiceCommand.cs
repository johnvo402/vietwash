using Application.Feature.Common.Projections.Services;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Services.Command.Update;

public class UpdateServiceCommand : IRequest<Result>
{
    [FromRoute(Name = RouterBase.Id)]
    public long ServiceId { get; set; } = default!;

    [FromBody]
    public ServiceModel Service { get; set; } = default!;
}
