using JohnChum.SharedKernel.Domain.Common.Specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Orders.Specifications
{
	public class ListOrderSpecification : Specification<Order>
	{
		public ListOrderSpecification()
		{
			Query
				.Include(x => x.OrderItems)
				.Include(x => x.OrderPayments)
				.AsNoTracking()
				.AsSplitQuery();
				//Chưa OrderBy

		}
	}
}
