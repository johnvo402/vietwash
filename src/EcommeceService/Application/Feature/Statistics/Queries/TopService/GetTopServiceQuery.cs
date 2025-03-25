using Application.Feature.Orders.Queries.List;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Statistics.Queries.TopService
{
    public class GetTopServiceQuery : IRequest<IEnumerable<GetTopServiceResponse>>;
}
