using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Equipments.Queries.Listl;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Equipments;
using Mediator;
using Domain.Aggregates.Equipments.Specifications;

namespace Application.Feature.Equipments.Queries.List
{
	public class ListEquipmentHandler(IUnitOfWork unitOfWork)
	: IRequestHandler<ListEquipmentQuery, Result<PaginationResponse<ListEquipmentResponse>>>
	{
		public async ValueTask<Result<PaginationResponse<ListEquipmentResponse>>> Handle(
			ListEquipmentQuery query,
			CancellationToken cancellationToken
		)
		{
			try
			{
				var validation = query.Validate<ListEquipmentQuery, ListEquipmentResponse>();

				if (validation != null)
				{
					return validation;
				}

				var response = await unitOfWork
					.DynamicReadOnlyRepository<Equipment>()
					.PagedListAsync(
						new ListEquipmentSpecification(),
						query,
						ListEquipmentMapping.Selector(),
						cancellationToken
					);

				return Result<PaginationResponse<ListEquipmentResponse>>.Success(response);
			}
			catch (Exception ex)
			{
				throw new Exception("Exception", ex);
			}
		}
	}

}
