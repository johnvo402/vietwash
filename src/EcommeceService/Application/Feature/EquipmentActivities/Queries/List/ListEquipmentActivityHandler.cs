using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Equipments;
using Mediator;
using Domain.Aggregates.Equipments.Specifications;

namespace Application.Feature.EquipmentActivities.Queries.List
{
	public class ListEquipmentActivityHandler(IUnitOfWork unitOfWork)
	: IRequestHandler<ListEquipmentActivityQuery, Result<PaginationResponse<ListEquipmentActivityResponse>>>
	{
		public async ValueTask<Result<PaginationResponse<ListEquipmentActivityResponse>>> Handle(
			ListEquipmentActivityQuery query,
			CancellationToken cancellationToken)
		{
			try
			{
				var validation = query.Validate<ListEquipmentActivityQuery, ListEquipmentActivityResponse>();

				if (validation != null)
				{
					return validation;
				}

				var response = await unitOfWork
					.DynamicReadOnlyRepository<EquipmentActivity>()
					.PagedListAsync(
						new ListEquipmentActivitySpecification(),
						query,
						ListEquipmentActivityMapping.Selector(),
						cancellationToken
					);

				return Result<PaginationResponse<ListEquipmentActivityResponse>>.Success(response);
			}
			catch (Exception ex)
			{
				throw new Exception("Exception", ex);
			}
		}
	}
}
