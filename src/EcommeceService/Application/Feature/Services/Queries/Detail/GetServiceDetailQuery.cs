using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Services.Queries.Detail
{
    public record GetServiceDetailQuery : IRequest<Result<GetServiceDetailResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long ServiceId { get; set; }
    }
}
