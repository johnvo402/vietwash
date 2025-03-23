using Mediator;

namespace Application.Feature.Services.Queries.Detail
{
    public record GetServiceDetailQuery(String ServiceId) : IRequest<GetServiceDetailResponse>;
}
