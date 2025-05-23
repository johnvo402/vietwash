using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Services;
using Domain.Aggregates.Services;
using FluentValidation;
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
			.GreaterThan(0).WithMessage("CategoryId must be a positive number.")
			.MustAsync(async (categoryId, cancellation) =>
			{
				var categoryExists = await _unitOfWork.Repository<Category>().AnyAsync(c => c.Id == categoryId, cancellation);
				return categoryExists;
			}).WithMessage("CategoryId does not exist.");
		//RuleFor(x => x.BranchId)
		//		.GreaterThan(0).WithMessage("BranchId must be a positive number.")
		//		.MustAsync(async (branchId, cancellation) =>
		//		{
		//			// Assuming a repository method to check if BranchId exists
		//			var branchExists = await _unitOfWork.Repository<Branch>().AnyAsync(b => b.Id == branchId, cancellation);
		//			return branchExists;
		//		}).WithMessage("BranchId does not exist.");
	}
}
