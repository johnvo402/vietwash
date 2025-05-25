using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Tariffs;
using Mediator;

namespace Application.Feature.Tariffs.Commands.Create
{
    public class CreateTariffHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<CreateTariffCommand, CreateTariffResponse>
    {
        public async ValueTask<CreateTariffResponse> Handle(CreateTariffCommand request, CancellationToken cancellationToken)
        {
            Tariff mappingTaiff = mapper.Map<Tariff>(request);
            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

                Tariff tariff = await unitOfWork
                    .Repository<Tariff>()
                    .AddAsync(mappingTaiff, cancellationToken);

                await unitOfWork.SaveAsync(cancellationToken);

                await unitOfWork.CommitAsync(cancellationToken);
                return new CreateTariffResponse
                {
                    Message = "Tariff created successfully"
                };
            }
            catch (Exception ex)
            {

                await unitOfWork.RollbackAsync(cancellationToken);
                return new CreateTariffResponse
                {
                    Message = ex.Message
                };
            }
        }
    }
}