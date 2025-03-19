using Application.Feature.Common.Projections.Services;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Services.Command.Update;

public class UpdateServiceCommand : IRequest<UpdateServiceResponse>
{
    [FromRoute(Name = RouterBase.Id)]
    public string ServiceId { get; set; } = string.Empty;

    [FromForm]
    public ServiceModel Service { get; set; } = default!;
}
