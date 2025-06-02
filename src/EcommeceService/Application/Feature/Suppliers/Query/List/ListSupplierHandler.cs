using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using System;
using System.Collections.Generic;
using Domain.Aggregates.Suppliers;
using Domain.Aggregates.Suppliers.Specifications;
using Application.Common.QueryStringProcessing;

namespace Application.Feature.Suppliers.Query.List
{
	public class ListSupplierHandler(IUnitOfWork unitOfWork)
	: IRequestHandler<ListSupplierQuery, PaginationResponse<ListSupplierResponse>>
	{
		public async ValueTask<PaginationResponse<ListSupplierResponse>> Handle(
			ListSupplierQuery query,
			CancellationToken cancellationToken
		) =>
			await unitOfWork.Repository<Supplier>().
				   PagedListAsync<ListSupplierResponse>(
				new ListSupplierSpecification(),
				query.ValidateQuery().ValidateFilter(typeof(ListSupplierResponse))

			);
	}
}
