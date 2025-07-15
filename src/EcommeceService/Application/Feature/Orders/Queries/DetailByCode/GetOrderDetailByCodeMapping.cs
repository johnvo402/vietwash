using Domain.Aggregates.Orders;

namespace Application.Feature.Orders.Queries.DetailByCode
{
    public static class GetOrderDetailByCodeMapping
    {
        public static GetOrderDetailByCodeResponse ToOrderDetailByCodeResponse(this Order order)
        {
            var response = new GetOrderDetailByCodeResponse();
            response.MappingFrom(order);
            return response;
        }
    }
}
