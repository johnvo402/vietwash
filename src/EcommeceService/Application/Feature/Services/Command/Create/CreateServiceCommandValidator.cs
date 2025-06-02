using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Services;
using Domain.Aggregates.Services;
using FluentValidation;
using Infrastructure.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;


namespace Application.Feature.Services.Command.Create;

public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
	private readonly IUnitOfWork _unitOfWork;

	private readonly IActionAccessorService _accessorService;

	public CreateServiceCommandValidator(
		IUnitOfWork unitOfWork,
		IActionAccessorService accessorService
	)
	{
		_unitOfWork = unitOfWork;
		_accessorService = accessorService;
		ApplyRules();
	}

	private void ApplyRules()
	{
		Include(new ServiceValidator(_unitOfWork, _accessorService));
		RuleFor(x => x.Name)
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
		RuleFor(x => x.CategoryId)
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
	}

		private async Task<bool> IsCategoryExistsAsync(string categoryId, CancellationToken cancellation)
	{
		return await _unitOfWork.Repository<Category>().AnyAsync(c => c.Id == categoryId, cancellation);
	}
}
