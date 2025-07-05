using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Tariffs.Specifications;
using Mediator;

namespace Application.Feature.Tariffs.Commands.Update
{
    public class UpdateTariffHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateTariffCommand, Result>
    {
        public async ValueTask<Result> Handle(
            UpdateTariffCommand command,
            CancellationToken cancellationToken
        )
        {
            Tariff? existingTariff = await unitOfWork
                .DynamicReadOnlyRepository<Tariff>()
                .FindByConditionAsync(
                    new GetTariffByIdWithoutIncludeSpecification(command.TariffId),
                    cancellationToken
                );
			if (existingTariff == null)
			{
				return Result.Failure(
					new NotFoundError(
						"Tariff not found",
						Messager.Create<Tariff>().Message(MessageType.Found).Negative().BuildMessage()
					)
				);
			}
			existingTariff.FromUpdateTariff(command.Tariff!);
            try
            {
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);

                await unitOfWork.Repository<Tariff>().UpdateAsync(existingTariff);

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
