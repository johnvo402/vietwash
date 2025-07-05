using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Mediator;
using Contracts.ApiWrapper;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Tariffs.Specifications;
using Application.Common.Errors;

namespace Application.Feature.Tariffs.Queries.Detail
{
	public class GetTariffDetailHandler(IUnitOfWork unitOfWork)
	: IRequestHandler<GetTariffDetailQuery, Result<GetTariffDetailResponse>>
	{
		public async ValueTask<Result<GetTariffDetailResponse>> Handle(
			GetTariffDetailQuery command,
			CancellationToken cancellationToken
		)
		{
			GetTariffDetailResponse? response = await unitOfWork
				.DynamicReadOnlyRepository<Tariff>()
				.FindByConditionAsync(
					new GetTariffWithIncludeByIdSpecification(command.TariffId),
					x => x.ToGetTariffDetailResponse(),
					cancellationToken
				);
			if (response == null)
			{
				return Result<GetTariffDetailResponse>.Failure(
					new NotFoundError(
						"Tariff not found",
						Messager.Create<Tariff>().Message(MessageType.Found).Negative().BuildMessage()
					)
				);
			}

			return Result<GetTariffDetailResponse>.Success(response);
		}
	}
}
