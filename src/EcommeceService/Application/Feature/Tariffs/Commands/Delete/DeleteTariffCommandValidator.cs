using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Domain.Aggregates.Tariffs;
using FluentValidation;
namespace Application.Feature.Tariffs.Commands.Delete
{
	public class DeleteTariffCommandValidator : AbstractValidator<DeleteTariffCommand>
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IActionAccessorService accessorService;

		public DeleteTariffCommandValidator(
			IUnitOfWork unitOfWork,
			IActionAccessorService accessorService
		)
		{
			this.unitOfWork = unitOfWork;
			this.accessorService = accessorService;
			ApplyRules();
		}
		private void ApplyRules()
		{
			RuleFor(x => x.TariffId)
				.NotEmpty()
				.WithState(x =>
						Messager
							.Create<Tariff>()
							.Property(x => x.Id)
							.Message(MessageType.Null)
							.Negative()
							.Build()
				);

		}
	}
}
