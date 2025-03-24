using Application.Feature.Common.Projections.Orders;
using Mediator;


namespace Application.Feature.Orders.Command.Create
{
	public class CreateOrderCommand : CreateOrderModel, IRequest<CreateOrderResponse>;

}
