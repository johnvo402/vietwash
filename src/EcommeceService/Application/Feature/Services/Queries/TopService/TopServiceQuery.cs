using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Services.Queries.TopService
{
    public class TopServiceQuery : IRequest<Result<IEnumerable<TopServiceResponse>>>;
}
