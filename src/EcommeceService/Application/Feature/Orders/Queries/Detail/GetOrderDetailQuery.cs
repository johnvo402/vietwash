using Mediator;


namespace Application.Feature.Orders.Queries.Detail
{
	public record GetOrderDetailQuery(Ulid orderId) : IRequest<GetOrderDetailResponse>
	{
	}
}
