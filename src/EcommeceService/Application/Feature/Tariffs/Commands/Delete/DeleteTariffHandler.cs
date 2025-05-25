using Application.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Tariffs.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;

namespace Application.Feature.Tariffs.Commands.Delete
{
    public class DeleteTariffHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteTariffCommand>
    {
        public async ValueTask<Unit> Handle(
        DeleteTariffCommand command,
        CancellationToken cancellationToken
    )
        {
            Tariff tariff =
                await unitOfWork
                    .Repository<Tariff>()
                    .FindByConditionAsync(
                        new GetTariffByIdWithoutIncludeSpecification(command.TariffId),
                        cancellationToken
                    )
                ?? throw new NotFoundException(
                    [Messager.Create<Tariff>().Message(MessageType.Found).Negative().BuildMessage()]
                );
            await unitOfWork.Repository<Tariff>().DeleteAsync(tariff);
            await unitOfWork.SaveAsync(cancellationToken);

            return Unit.Value;
        }
    }
}