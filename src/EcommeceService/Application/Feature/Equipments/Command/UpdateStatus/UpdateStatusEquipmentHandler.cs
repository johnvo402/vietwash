using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Domain.Aggregates.Equipments.Specifications;
using Domain.Aggregates.Equipments;
using Mediator;
using Contracts.ApiWrapper;

namespace Application.Feature.Equipments.Command.UpdateStatus
{
	public class UpdateStatusEquipmentHandler(IUnitOfWork unitOfWork)
		: IRequestHandler<UpdateStatusEquipmentCommand, Result>
	{
		public async ValueTask<Result> Handle(
			UpdateStatusEquipmentCommand command,
			CancellationToken cancellationToken
		)
		{
			try
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
							"EquipmentActivity not found",
							Messager.Create<Equipment>().Message(MessageType.Found).Negative().BuildMessage()
						)
					);
				}

				using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

				await unitOfWork.Repository<Equipment>().UpdateAsync(existingEquipment);
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
