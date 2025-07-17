using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Tariffs;
using FluentValidation;
using Domain.Aggregates.Tariffs;
using Contracts.Common.Messages;
using Domain.Aggregates.Services;

namespace Application.Feature.Common.Validators.Tariffs
{
	public class TariffValidator : AbstractValidator<TariffModel>
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IActionAccessorService accessorService;
		public TariffValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
		{
			this.unitOfWork = unitOfWork;
			this.accessorService = accessorService;
			ApplyRules();
		}
		private void ApplyRules()
		{
			RuleFor(t => t.Name)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<Tariff>()
						.Property(x => x.Name)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				)
				.MaximumLength(256)
				.WithState(x =>
					Messager
					.Create<Tariff>()
					.Property(x => x.Name)
					.Message(MessageType.MaximumLength)
					.Build());
			RuleForEach(x => x.ServiceTariffs)
				.ChildRules(item =>
				{
					item.RuleFor(x => x.ServiceId)
						.NotEmpty()
						.WithState(x =>
							Messager
								.Create<ServiceTariffModel>(nameof(ServiceTariff))
								.Property(x => x.ServiceId)
								.Message(MessageType.Null)
								.Negative()
								.Build()
						)
						.MustAsync(IsServiceExistAsync)
						.WithState(x =>
							Messager
								.Create<ServiceTariffModel>(nameof(ServiceTariff))
								.Property(x => x.ServiceId)
								.Message(MessageType.Existence)
								.Negative()
								.Build()
						);
					item.RuleFor(x => x.UnitRelationId)
						.NotEmpty()
						.WithState(x =>
							Messager
								.Create<ServiceTariffModel>(nameof(ServiceTariff))
								.Property(x => x.UnitRelationId)
								.Message(MessageType.Null)
								.Negative()
								.Build()
						)
						.MustAsync(
							(model, unitRelationId, ct) => IsUnitRelationBelongToService(model.ServiceId, unitRelationId, ct)
						)
						.WithState(x =>
							Messager
								.Create<ServiceTariffModel>(nameof(ServiceTariff))
								.Property(x => x.UnitRelationId)
								.Message(MessageType.Existence)
								.Negative()
								.Build()
						);

					item.RuleFor(x => x.Price)
						.GreaterThan(0)
						.WithState(x =>
							Messager
								.Create<ServiceTariffModel>(nameof(ServiceTariff))
								.Property(x => x.Price)
								.Message(MessageType.GreaterThan)
								.Negative()
								.Build()
						);
				});
		}
		private async Task<bool> IsServiceExistAsync(long serviceId, CancellationToken ct)
		{
			return await unitOfWork.Repository<Service>().AnyAsync(s => s.Id == serviceId && !s.Disable, ct);
		}
		private async Task<bool> IsUnitRelationBelongToService(long serviceId, long unitRelationId, CancellationToken ct)
		{
			return await unitOfWork.Repository<UnitRelation>().AnyAsync(x => x.Id == unitRelationId && x.ServiceId == serviceId, ct);
		}
	}
}