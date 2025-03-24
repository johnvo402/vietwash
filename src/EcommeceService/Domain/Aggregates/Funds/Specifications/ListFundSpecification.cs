
using Domain.Aggregates.Orders.Enums;
using JohnChum.SharedKernel.Domain.Common.Specs;
using JohnChum.SharedKernel.Domain.Common.Specs.Interfaces;

namespace Domain.Aggregates.Funds.Specifications
{
	public class ListFundSpecification : Specification<Fund>
	{
		public ListFundSpecification()
		{
			Query
				.Include(x => x.FundType)
				.Include(x => x.FundBehavior)
				.AsNoTracking()
				.AsSplitQuery();
		}
	}
}
