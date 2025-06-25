using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Tariffs;
using Mediator;

namespace Application.Feature.Tariffs.Commands.Create
{
    public class CreateTariffHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<CreateTariffCommand, Result>
    {
        public async ValueTask<Result> Handle(
            CreateTariffCommand request,
            CancellationToken cancellationToken
        )
        {
            Tariff mappingTariff = request.ToEntityCreate();

            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                Tariff tariff = await unitOfWork
                    .Repository<Tariff>()
                    .AddAsync(mappingTariff, cancellationToken);

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
