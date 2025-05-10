using Application.Feature.Common.Projections.Orders;
using Application.Feature.Common.Projections.Services;
using Application.Feature.Common.Projections.Units;
using Application.Feature.Orders.Command.Create;
using Application.Feature.Services.Command.Update;
using AutoMapper;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Services;


namespace Application.Feature.Orders.Command.Update
{
	public class UpdateOrderMapping : Profile
	{
		public UpdateOrderMapping() {
			CreateMap<UpdateOrderModel, Order>()
				.ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src =>
					src.OrderItems.Select(item => new OrderItem
					{
						ServiceId = Ulid.Parse(item.ServiceId),
						UnitRelationId = Ulid.Parse(item.UnitRelationId),
						Price = item.Price
					}).ToList()));

			CreateMap<Order, UpdateOrderResponse>()
				.IncludeBase<Order, OrderProjection>();

		}	
	}
}
