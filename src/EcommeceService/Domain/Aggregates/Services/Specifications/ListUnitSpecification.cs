using JohnChum.SharedKernel.Domain.Common.Specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Services.Specifications
{
	public class ListUnitSpecification : Specification<Unit>
	{
		public ListUnitSpecification()
		{
			Query
				.AsNoTracking()
				.AsSplitQuery();
			string key = GetUniqueCachedKey();
			Query.EnableCache(key);
		}
	}
}
