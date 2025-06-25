using Application.Feature.Common.Mapping.Orders;
using Domain.Aggregates.Orders;

namespace Application.Feature.Orders.Queries.Detail
{
    public static class GetOrderDetailMapping
    {
        public static GetOrderDetailResponse ToOrderDetailResponse(this Order order)
        {
            var response = new GetOrderDetailResponse();
            response.MappingFrom(order);
            return response;
        }
    }
}
