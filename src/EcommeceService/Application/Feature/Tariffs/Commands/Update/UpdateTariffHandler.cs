using System;
using System.Collections.Generic;
using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Tariffs.Specifications;
using Mediator;

namespace Application.Feature.Tariffs.Commands.Update
{
    public class UpdateTariffHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateTariffCommand, Result<UpdateTariffResponse>>
    {
        public async ValueTask<Result<UpdateTariffResponse>> Handle(
            UpdateTariffCommand command,
            CancellationToken cancellationToken
        )
        {
            Tariff? tariff = await unitOfWork
                .DynamicReadOnlyRepository<Tariff>()
                .FindByConditionAsync(
                    new GetTariffByIdWithoutIncludeSpecification(long.Parse(command.TariffId)),
                    cancellationToken
                );
            if (tariff == null)
            {
                return Result<UpdateTariffResponse>.Failure(
                    new NotFoundError(
                        "Your resource is not found",
                        Messager
                            .Create<Tariff>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }
            tariff.FromUpdateTariff(command.Tariff!);
            try
            {
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);

                await unitOfWork.Repository<Tariff>().UpdateAsync(tariff);

                await unitOfWork.SaveAsync(cancellationToken);

                await unitOfWork.CommitAsync(cancellationToken);

                return Result<UpdateTariffResponse>.Success(
                    new UpdateTariffResponse { Message = "Success" }
                );
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
