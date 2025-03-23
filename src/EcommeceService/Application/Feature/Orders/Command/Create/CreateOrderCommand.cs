using Application.Feature.Common.Projections.Orders;
using Mediator;


namespace Application.Feature.Orders.Command.Create
{
	public class CreateOrderCommand : CreateOrderModel, IRequest;

	//public class CreateOrderResponse
	//{
	//	public string Id { get; set; }
	//	public string Code { get; set; } = string.Empty;
	//	public decimal Total { get; set; }
	//	public string Status { get; set; } = string.Empty;
	//	public DateTimeOffset OrderDate { get; set; }
	//}
}
