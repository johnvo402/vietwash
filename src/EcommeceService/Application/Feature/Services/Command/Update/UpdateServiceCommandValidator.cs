using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Domain.Aggregates.Services;
using FluentValidation;
using Infrastructure.UnitOfWorks;
using Contracts.Common.Messages;
using Application.Feature.BranchProducts.Command.Update;

namespace Application.Feature.Services.Command.Update
{
    public class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IActionAccessorService accessorService;

        public UpdateServiceCommandValidator(
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
			RuleFor(x => x.ServiceId)
				.NotEmpty()
				.WithState(x =>
						Messager
							.Create<Service>()
							.Property(x => x.CategoryId)
							.Message(MessageType.Null)
							.Negative()
							.Build()
				)
				.MustAsync(IsServiceExistsAsync)
				.WithState(x =>
						Messager
							.Create<Service>()
							.Property(x => x.Id)
							.Message(MessageType.Found)
							.Negative()
							.Build()
				);
			RuleFor(x => x.Service.Name)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<Service>()
						.Property(x => x.Name)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				)
				.MaximumLength(256)
				.WithState(x =>
					Messager
						.Create<Service>()
						.Property(x => x.Name)
						.Message(MessageType.MaximumLength)
						.Build()
				);
			RuleFor(x => x.Service.CategoryId)
				.NotEmpty()
				.WithState(x =>
						Messager
							.Create<Service>()
							.Property(x => x.CategoryId)
							.Message(MessageType.Null)
							.Negative()
							.Build()
				)
				.MustAsync(IsCategoryExistsAsync)
				.WithState(_ =>
						Messager
							.Create<Service>()
							.Property(x => x.CategoryId)
							.Message(MessageType.Found)
							.Negative()
							.Build()
				);
			RuleForEach(x => x.Service.UnitRelations)
					.MustAsync(async (command, unit, cancellationToken) =>
						unit.Id == 0 // UnitRelation mới thì bỏ qua
						|| await IsUnitRelationBelongToServiceAsync(command, unit.Id, cancellationToken)
					)
					.WithState(_ =>
						Messager.Create<UnitRelation>()
							.Property(x => x.Id)
							.Message(MessageType.Found)
							.Negative()
							.Build()
					);
		}
		private async Task<bool> IsServiceExistsAsync(long serviceId, CancellationToken cancellation)
		{
			return await unitOfWork.Repository<Service>().AnyAsync(s => s.Id == serviceId, cancellation);
		}

		private async Task<bool> IsCategoryExistsAsync(string categoryId, CancellationToken cancellation)
		{
			return await unitOfWork.Repository<Category>().AnyAsync(c => c.Id == categoryId, cancellation);
		}
		private async Task<bool> IsUnitRelationBelongToServiceAsync(UpdateServiceCommand command, long unitRelationId, CancellationToken cancellation)
		{
			return await unitOfWork.Repository<UnitRelation>()
				.AnyAsync(x => x.Id == unitRelationId && x.ServiceId == command.ServiceId, cancellation);
		}
	}
}
