using Domain.Aggregates.Orders.Enums;
using JohnChum.SharedKernel.Domain.Common.Specs;


namespace Domain.Aggregates.Orders.Specifications
{
	public class ListOrderSpecification : Specification<Order>
	{
		public ListOrderSpecification()
		{
			Query
				.Where(x => x.Status != OrderStatus.Cancelled)
				.Include(x => x.OrderItems)
				.Include(x => x.OrderPayments)
				.AsNoTracking()
				.AsSplitQuery();
		}
	}
}
