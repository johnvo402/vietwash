using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Specifications;
using Mediator;

namespace Application.Feature.Equipments.Command.Update
{
    public class UpdateEquipmentHandler(IUnitOfWork unitOfWork)
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

            existingEquipment.FromUpdateModel(command.Equipment);

            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                await unitOfWork.Repository<Equipment>().UpdateAsync(existingEquipment);

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
