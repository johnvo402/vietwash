using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Tariffs.Specifications;
using Mediator;

namespace Application.Feature.Tariffs.Commands.Delete
{
    public class DeleteTariffHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteTariffCommand, Result>
    {
        public async ValueTask<Result> Handle(
            DeleteTariffCommand command,
            CancellationToken cancellationToken
        )
        {
            Tariff? tariff = await unitOfWork
                .Repository<Tariff>()
                .FindByIdAsync(
                    new GetTariffByIdWithoutIncludeSpecification(command.TariffId),
                    cancellationToken
                );
            if (tariff == null)
            {
                return Result.Failure(
                    new NotFoundError(
                        "Tariff not found",
                        Messager
                            .Create<Tariff>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }
            await unitOfWork.Repository<Tariff>().DeleteAsync(tariff);
            await unitOfWork.SaveAsync(cancellationToken);

            return Result.Success();
        }
    }
}
