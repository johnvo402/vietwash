using Application.Feature.Common.Projections.Orders;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Orders.Command.Create
{
    public class CreateOrderCommand : OrderModel, IRequest<Result<CreateOrderResponse>>;
}
