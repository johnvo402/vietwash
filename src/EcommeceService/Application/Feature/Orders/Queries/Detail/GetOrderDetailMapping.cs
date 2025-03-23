using Application.Feature.Common.Projections.Orders;
using AutoMapper;
using Domain.Aggregates.Orders;

namespace Application.Feature.Orders.Queries.Detail
{
	public class GetOrderDetailMapping : Profile
	{
		public GetOrderDetailMapping()
		{

			CreateMap<Order, GetOrderDetailResponse>().IncludeBase<Order, OrderDetailProjection>();
			CreateMap<OrderPayment, OrderPaymentProjection>();
		}
	}
}
