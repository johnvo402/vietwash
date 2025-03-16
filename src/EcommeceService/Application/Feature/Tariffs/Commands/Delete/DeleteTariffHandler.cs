using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Tariffs.Specifications;
using Domain.Aggregates.Users.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;

namespace Application.Feature.Tariffs.Commands.Delete
{
    public class DeleteTariffHandler(IUnitOfWork unitOfWork, IMediaUpdateService<Tariff> MediaUpdateService)
    : IRequestHandler<DeleteTariffCommand>
    {
        public async ValueTask<Unit> Handle(
        DeleteTariffCommand command,
        CancellationToken cancellationToken
    )
    {
        Tariff Tariff =
            await unitOfWork
                .Repository<Tariff>()
                .FindByConditionAsync(
                    new GetTariffByIdWithoutIncludeSpecification(command.TariffId),
                    cancellationToken
                )
            ?? throw new NotFoundException(
                [Messager.Create<Tariff>().Message(MessageType.Found).Negative().BuildMessage()]
            );
        await unitOfWork.Repository<Tariff>().DeleteAsync(Tariff);
        await unitOfWork.SaveAsync(cancellationToken);

        return Unit.Value;
    }
    }
}