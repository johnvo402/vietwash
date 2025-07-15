using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Specifications;
using Mediator;

namespace Application.Feature.EquipmentActivities.Queries.Detail
{
	public class GetEquipmentActivityDetailHandler(IUnitOfWork unitOfWork)
		: IRequestHandler<GetEquipmentActivityDetailQuery, Result<GetEquipmentActivityDetailResponse>>
	{
		public async ValueTask<Result<GetEquipmentActivityDetailResponse>> Handle(
			GetEquipmentActivityDetailQuery request,
			CancellationToken cancellationToken
		)
		{
			var equipmentActivity = await unitOfWork
				.DynamicReadOnlyRepository<EquipmentActivity>()
				.FindByConditionAsync(
					new GetEquipmentActivityByIdSpecification(request.EquipmentActivityId),
					x => x.ToEquipmentActivityDetailResponse(),
					cancellationToken
				);
			if (equipmentActivity == null)
			{
				return Result<GetEquipmentActivityDetailResponse>.Failure(
					new NotFoundError(
						"EquipmentActivity not found",
						Messager
							.Create<EquipmentActivity>()
							.Message(MessageType.Found)
							.Negative()
							.BuildMessage()
					)
				);
			}

			return Result<GetEquipmentActivityDetailResponse>.Success(equipmentActivity);
		}
	}
}
