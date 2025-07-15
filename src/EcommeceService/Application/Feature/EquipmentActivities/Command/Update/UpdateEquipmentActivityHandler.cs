using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Mediator;
using System.Data.Common;
using Application.Common.Interfaces.Services;
using Contracts.ApiWrapper;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Specifications;
using Domain.Aggregates.Users;

namespace Application.Feature.EquipmentActivities.Command.Update
{
	public class UpdateEquipmentActivityHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
	: IRequestHandler<UpdateEquipmentActivityCommand, Result>
	{
		public async ValueTask<Result> Handle(
			UpdateEquipmentActivityCommand command,
			CancellationToken cancellationToken
		)
		{
			var staff = await unitOfWork
					.Repository<User>()
					.FindByIdAsync((long)currentAccount.Id!, cancellationToken);
			if (staff == null)
			{
				return Result.Failure(
					new NotFoundError(
						"Staff not found",
						Messager
							.Create<User>()
							.Message(MessageType.Found)
							.Negative()
							.BuildMessage()
					)
				);
			}

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

			existingEquipmentActivity.FromUpdateModel(command.EquipmentActivity, staff.Id, staff.Code);

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

				existingEquipmentActivity.Status = command.Status.Value;
			}

			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

				await unitOfWork.Repository<EquipmentActivity>().UpdateAsync(existingEquipmentActivity);

				await unitOfWork.SaveAsync(cancellationToken);
				await unitOfWork.CommitAsync(cancellationToken);

				return Result.Success();
			}
			catch
			{
				await unitOfWork.RollbackAsync(cancellationToken);
				throw;
			}
		}
	}
}
