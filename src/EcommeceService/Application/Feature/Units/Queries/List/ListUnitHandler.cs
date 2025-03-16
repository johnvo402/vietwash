

using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.QueryStringProcessing;
using AutoMapper;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using Infrastructure.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Unit = Domain.Aggregates.Services.Unit;

namespace Application.Feature.Units.Queries.List
{
	public class ListUnitHandler(IUnitOfWork unitOfWork, IMapper mapper)
		: IRequestHandler<ListUnitQuery, PaginationResponse<ListUnitResponse>>
	{
		public async ValueTask<PaginationResponse<ListUnitResponse>> Handle(
			ListUnitQuery query, CancellationToken cancellationToken
			) =>
			await unitOfWork
				.CachedRepository<Unit>()
				.CursorPagedListAsync<ListUnitResponse>(
					new ListUnitSpecification(),
					query.ValidateQuery().ValidateFilter(typeof(ListUnitResponse))
				);

	}
}
