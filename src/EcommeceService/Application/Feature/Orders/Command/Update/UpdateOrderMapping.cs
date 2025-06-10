using Application.Feature.Common.Projections.Orders;
using AutoMapper;
using Domain.Aggregates.Orders;


namespace Application.Feature.Orders.Command.Update
{
    public class UpdateOrderMapping : Profile
    {
        public UpdateOrderMapping()
        {
            CreateMap<UpdateOrderModel, Order>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src =>
                    src.OrderItems.Select(item => new OrderItem
                    {
                        ServiceId = item.ServiceId,
                        UnitRelationId = item.UnitRelationId,
                        Price = item.Price
                    }).ToList()));

            CreateMap<Order, UpdateOrderResponse>()
                .IncludeBase<Order, OrderProjection>();

        }
    }
}
