using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Mediator;
using Contracts.ApiWrapper;
using Domain.Aggregates.Equipments.Specifications;
using Domain.Aggregates.Equipments;
using StackExchange.Redis;
using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.EquipmentActivities.Command.UpdateStatus
{
	public class UpdateStatusEquipmentActivityHandler(IUnitOfWork unitOfWork)
		: IRequestHandler<UpdateStatusEquipmentActivityCommand, Result>
	{
		public async ValueTask<Result> Handle(
			UpdateStatusEquipmentActivityCommand command,
			CancellationToken cancellationToken
		)
		{
			try
			{
				EquipmentActivity? existingEquipmentActivity = await unitOfWork
				.DynamicReadOnlyRepository<EquipmentActivity>()
				.FindByConditionAsync(
					new GetEquipmentActivityWithIncludeByIdSpecification(command.EquipmentActivityId),
					cancellationToken
				);
				if (existingEquipmentActivity == null)
				{
					return Result.Failure(
						new NotFoundError(
							"EquipmentActivity not found",
							Messager.Create<EquipmentActivity>().Message(MessageType.Found).Negative().BuildMessage()
						)
					);
				}

				if (command.Status.HasValue)
				{
					if (command.Status.Value < existingEquipmentActivity.Status)
						return Result.Failure(
							new BadRequestError(
								"Status invalid",
								Messager
									.Create<EquipmentActivity>()
									.Property(x => x.Status)
									.Message(MessageType.Valid)
									.Negative()
									.BuildMessage()
							)
						);

					existingEquipmentActivity.UpdateStatus(command.Status.Value);
				}

				using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

				await unitOfWork.Repository<EquipmentActivity>().UpdateAsync(existingEquipmentActivity);
				await unitOfWork.SaveAsync(cancellationToken);

				await unitOfWork.CommitAsync(cancellationToken);
				return Result.Success();
			}
			catch (Exception)
			{
				await unitOfWork.RollbackAsync(cancellationToken);
				throw;
			}
		}
	}
}
