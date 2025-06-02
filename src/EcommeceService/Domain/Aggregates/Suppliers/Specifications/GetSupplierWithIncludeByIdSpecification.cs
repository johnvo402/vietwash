using Domain.Aggregates.Enums;
using Domain.Aggregates.Services;
using Domain.Aggregates.Suppliers.Enum;
using JohnChum.SharedKernel.Domain.Common.Specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Suppliers.Specifications
{
	public class GetSupplierWithIncludeByIdSpecification : Specification<Supplier>
	{
		public GetSupplierWithIncludeByIdSpecification(long id)
		{
			Query
				.Where(x => x.Id == id && x.Status == SupplierStatus.Active)
				.AsNoTracking();
		}
	}
}
