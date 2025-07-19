using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Equipments.Specifications;
using Domain.Aggregates.Users;
using Mediator;
using System.Data.Common;

namespace Application.Feature.Equipments.Command.CreateActivities
{
	public class CreateEquipmentActivityHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
		: IRequestHandler<CreateEquipmentActivityCommand, Result>
	{
		public async ValueTask<Result> Handle(
			CreateEquipmentActivityCommand request,
			CancellationToken cancellationToken
		)
		{
			Equipment? existingEquipment = await unitOfWork
				.DynamicReadOnlyRepository<Equipment>()
				.FindByConditionAsync(
					new GetEquipmentWithIncludeByIdSpecification(request.Id),
					cancellationToken
				);
			if (existingEquipment == null)
			{
				return Result.Failure(
					new NotFoundError(
						"Equipment not found",
						Messager
							.Create<Equipment>()
							.Message(MessageType.Found)
							.Negative()
							.BuildMessage()
					)
				);
			}

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

			existingEquipment.Status = EquipmentStatus.Active;
			existingEquipment.LastMaintenanceOrRepairDate = DateTimeOffset.UtcNow;
			var activity = request.ToEquipmentActivity(staff);
			existingEquipment.EquipmentActivities.Add(activity);

			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
					cancellationToken
				);

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
