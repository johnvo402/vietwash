using JohnChum.SharedKernel.Domain.Common.Specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Orders.Specifications
{
	public class GetOrderByIdSpecification : Specification<Order>
	{
		public GetOrderByIdSpecification(Ulid id)
		{
			Query
				.Where(x => x.Id == id)
				.Include(x => x.OrderItems)
				.Include(x => x.OrderPayments)
				.AsSplitQuery();
		}
	}
}
