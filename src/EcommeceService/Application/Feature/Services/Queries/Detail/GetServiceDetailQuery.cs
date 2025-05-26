using Mediator;

namespace Application.Feature.Services.Queries.Detail
{
    public record GetServiceDetailQuery(long ServiceId) : IRequest<GetServiceDetailResponse>;
}
