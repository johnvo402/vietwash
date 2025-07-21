using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Equipments;
using Mediator;

namespace Application.Feature.Equipments.Command.Update
{
    public class UpdateEquipmentHandler(IUnitOfWork unitOfWork, IMediaUpdateService media)
        : IRequestHandler<UpdateEquipmentCommand, Result>
    {
        public async ValueTask<Result> Handle(
            UpdateEquipmentCommand command,
            CancellationToken cancellationToken
        )
        {
            Equipment? existingEquipment = await unitOfWork
                .Repository<Equipment>()
                .FindByIdAsync(command.EquipmentId, cancellationToken);
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
            string? oldImage = null;
            var newImage = command.Equipment.Image;
            if (!string.IsNullOrEmpty(newImage))
            {
                oldImage = existingEquipment.Image;
            }
            existingEquipment.FromUpdateModel(command.Equipment);

            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                await unitOfWork.Repository<Equipment>().UpdateAsync(existingEquipment);

                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
                await media.DeleteMediaAsync(oldImage);
                return Result.Success();
            }
            catch
            {
                await media.DeleteMediaAsync(newImage);
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
