using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Suppliers.Query.List;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Suppliers.Specifications;
using Domain.Aggregates.Suppliers;
using Mediator;
using Contracts.Common.QueryStringProcessing;
using Domain.Aggregates.Products;
using Domain.Aggregates.Products.Specifications;

namespace Application.Feature.Products.Queries.List
{
	public class ListProductHandler(IUnitOfWork unitOfWork)
		: IRequestHandler<ListProductQuery, Result<PaginationResponse<ListProductResponse>>>
	{
		public async ValueTask<Result<PaginationResponse<ListProductResponse>>> Handle(
			ListProductQuery query, 
			CancellationToken cancellationToken
		)
		{
			var validation = query.Validate<ListProductQuery, ListProductResponse>();

			if (validation != null)
			{
				return validation;
			}

			var response = await unitOfWork
				.DynamicReadOnlyRepository<Product>()
				.CursorPagedListAsync(
					new ListProductSpecification(),
					query,
					ListProductMapping.Selector(),
					cancellationToken: cancellationToken
				);
			return Result<PaginationResponse<ListProductResponse>>.Success(response);

		}
	}
}
