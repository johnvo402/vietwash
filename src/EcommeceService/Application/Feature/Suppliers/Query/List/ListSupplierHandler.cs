using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Domain.Aggregates.Suppliers;
using Domain.Aggregates.Suppliers.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.QueryStringProcessing;
using Application.Feature.Services.Queries.List;
using Domain.Aggregates.Services.Specifications;
using Domain.Aggregates.Services;

namespace Application.Feature.Suppliers.Query.List
{
	public class ListSupplierHandler(IUnitOfWork unitOfWork)
	: IRequestHandler<ListSupplierQuery, PaginationResponse<ListSupplierResponse>>
	{
		public async ValueTask<PaginationResponse<ListSupplierResponse>> Handle(
			ListSupplierQuery query,
			CancellationToken cancellationToken
		)
		{
			try
			{
				return await unitOfWork.Repository<Supplier>().
			   PagedListAsync<ListSupplierResponse>(
			new ListSupplierSpecification(),
			query.ValidateQuery().ValidateFilter(typeof(ListSupplierResponse))

		);
			}
			catch (Exception ex)
			{
				throw new Exception("Exception", ex);
			}
		}


	}
}
