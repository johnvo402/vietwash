using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Users;
using Mediator;
using System.Data.Common;

namespace Application.Feature.EquipmentActivities.Command.Create
{
	public class CreateEquipmentActivityHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
		: IRequestHandler<CreateEquipmentActivityCommand, Result>
	{
		public async ValueTask<Result> Handle(
			CreateEquipmentActivityCommand request,
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
			var enitty = request.ToEntity(staff.Id, staff.Code);

			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
					cancellationToken
				);

				var response = await unitOfWork
					.Repository<EquipmentActivity>()
					.AddAsync(enitty, cancellationToken);
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
