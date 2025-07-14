using System.Data.Common;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Equipments;
using Mediator;

namespace Application.Feature.Equipments.Command.Create
{
    public class CreateEquipmentHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<CreateEquipmentCommand, Result>
    {
        public async ValueTask<Result> Handle(
            CreateEquipmentCommand request,
            CancellationToken cancellationToken
        )
        {
            Equipment mappingEquipment = request.ToEntity();

            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                Equipment equipment = await unitOfWork
                    .Repository<Equipment>()
                    .AddAsync(mappingEquipment, cancellationToken);

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
