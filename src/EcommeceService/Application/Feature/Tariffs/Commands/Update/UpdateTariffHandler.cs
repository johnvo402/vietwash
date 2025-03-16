using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Tariffs.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;

namespace Application.Feature.Tariffs.Commands.Update
{
    public class UpdateTariffHandler(IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<UpdateTariffCommand, UpdateTariffResponse>
    {
        public async ValueTask<UpdateTariffResponse> Handle(
            UpdateTariffCommand command,
            CancellationToken cancellationToken
        )
        {
            Tariff Tariff =
                await unitOfWork
                    .Repository<Tariff>()
                    .FindByConditionAsync(
                        new GetTariffByIdWithoutIncludeSpecification(Ulid.Parse(command.TariffId)),
                        cancellationToken
                    )
                ?? throw new NotFoundException(
                    [Messager.Create<Tariff>().Message(MessageType.Found).Negative().BuildMessage()]
                );


            mapper.Map(command.Tariff, Tariff);

            // update default claim

            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

                await unitOfWork.Repository<Tariff>().UpdateAsync(Tariff);
                await unitOfWork.SaveAsync(cancellationToken);


                await unitOfWork.Repository<Tariff>().UpdateAsync(
                    Tariff
                );
                await unitOfWork.CommitAsync(cancellationToken);

                return mapper.Map<UpdateTariffResponse>(Tariff);
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}