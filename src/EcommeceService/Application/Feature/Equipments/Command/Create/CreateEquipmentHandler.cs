using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Equipments;
using Mediator;
using System.Data.Common;

namespace Application.Feature.Equipments.Command.Create
{
	public class CreateEquipmentHandler(
		IUnitOfWork unitOfWork,
		IMediaUpdateService mediaUpdateService
	) : IRequestHandler<CreateEquipmentCommand, Result>
	{
		public async ValueTask<Result> Handle(CreateEquipmentCommand request, CancellationToken cancellationToken)
		{
			Equipment mappingEquipment = request.ToEntity();

			string? equipmentImage = null;
			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
					cancellationToken
				);

				Equipment equipment = await unitOfWork
					.Repository<Equipment>()
					.AddAsync(mappingEquipment, cancellationToken);
				equipmentImage = equipment.Image;

				await unitOfWork.SaveAsync(cancellationToken);
				await unitOfWork.CommitAsync(cancellationToken);
				return Result.Success();
			}
			catch (Exception)
			{
				if (!string.IsNullOrEmpty(equipmentImage))
				{
					await mediaUpdateService.DeleteAvatarAsync(equipmentImage);
				}
				await unitOfWork.RollbackAsync(cancellationToken);
				throw;
			}
		}
	}
}
