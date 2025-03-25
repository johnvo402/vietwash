using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.QueryStringProcessing;
using Application.Feature.Orders.Queries.List;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Specifications;
using Domain.Aggregates.Orders.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Funds.Queries.List
{
	public class ListFundHandler(IUnitOfWork unitOfWork)
	: IRequestHandler<ListFundQuery, PaginationResponse<ListFundResponse>>
	{
		public async ValueTask<PaginationResponse<ListFundResponse>> Handle(ListFundQuery request, CancellationToken cancellationToken)
		{
			return await unitOfWork
				.Repository<Fund>()
				.PagedListAsync<ListFundResponse>(
					new ListFundSpecification(DateTime.Parse(request.From), DateTime.Parse(request.To)),
					request.ValidateQuery().ValidateFilter(typeof(ListFundResponse))
				);
		}
	}
}
