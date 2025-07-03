using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Mediator;
using System.Data.Common;
using Contracts.ApiWrapper;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Specifications;

namespace Application.Feature.Equipments.Command.Update
{
	public class UpdateEquipmentHandler(IUnitOfWork unitOfWork, IMediaUpdateService mediaUpdateService)
	: IRequestHandler<UpdateEquipmentCommand, Result>
	{
		public async ValueTask<Result> Handle(
			UpdateEquipmentCommand command,
			CancellationToken cancellationToken
		)
		{
			Equipment? existingEquipment = await unitOfWork
				.DynamicReadOnlyRepository<Equipment>()
				.FindByConditionAsync(
					new GetEquipmentWithIncludeByIdSpecification(command.EquipmentId),
					cancellationToken
				);
			if (existingEquipment == null)
			{
				return Result.Failure(
					new NotFoundError(
						"Equipment not found",
						Messager.Create<Equipment>().Message(MessageType.Found).Negative().BuildMessage()
					)
				);
			}

			string? oldEquipmentImage = existingEquipment.Image;

			existingEquipment.FromUpdateModel(command.Equipment);

			string? newEquipmentImage = command.Equipment.Image;
			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

				await unitOfWork.Repository<Equipment>().UpdateAsync(existingEquipment);

				await unitOfWork.SaveAsync(cancellationToken);
				await unitOfWork.CommitAsync(cancellationToken);

				if (!string.IsNullOrEmpty(oldEquipmentImage))
				{
					await mediaUpdateService.DeleteAvatarAsync(oldEquipmentImage);
				}

				return Result.Success();
			}
			catch
			{
				if (!string.IsNullOrEmpty(newEquipmentImage))
				{
					await mediaUpdateService.DeleteAvatarAsync(newEquipmentImage);
				}
				await unitOfWork.RollbackAsync(cancellationToken);
				throw;
			}
		}
	}
}