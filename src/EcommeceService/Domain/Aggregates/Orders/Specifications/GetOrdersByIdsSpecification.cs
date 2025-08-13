using Domain.Aggregates.Orders.Enums;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Orders.Specifications
{
    public class GetOrdersByIdsSpecification : Specification<Order>
    {
        public GetOrdersByIdsSpecification(List<long> id)
        {
            Query
                .Where(x => id.Contains(x.Id))
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Service)
                .Include(x => x.OrderEquipments)
                .ThenInclude(x => x.Equipment)
                .Include(x => x.Customer)
                .Include(x => x.Staff)
                .Include(x => x.Tariff)
                .AsSplitQuery();
        }
    }
}
