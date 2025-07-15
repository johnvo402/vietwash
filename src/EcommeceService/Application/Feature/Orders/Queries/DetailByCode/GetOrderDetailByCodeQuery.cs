using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Orders.Queries.DetailByCode
{
    public record GetOrderDetailByCodeQuery : IRequest<Result<GetOrderDetailByCodeResponse>>
    {
        public string Code { get; set; } = default!;
    }
}
