using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Services.Queries.Detail
{
    public record GetServiceDetailQuery([FromRoute(Name = RouterBase.Id)] long ServiceId)
        : IRequest<Result<GetServiceDetailResponse>>;
}
