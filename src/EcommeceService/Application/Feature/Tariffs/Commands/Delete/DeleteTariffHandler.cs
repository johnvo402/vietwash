using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Tariffs.Specifications;
using Mediator;
using System.Data.Common;

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
			Tariff? existingTariff = await unitOfWork
						.Repository<Tariff>()
						.FindByIdAsync(command.TariffId);
			if (existingTariff == null)
			{
				return Result.Failure(
					new NotFoundError(
						"Tariff not found",
						Messager.Create<Tariff>().Message(MessageType.Found).Negative().BuildMessage()
					)
				);
			}
			existingTariff.Disable = true;

			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

				await unitOfWork.Repository<Tariff>().UpdateAsync(existingTariff);
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
