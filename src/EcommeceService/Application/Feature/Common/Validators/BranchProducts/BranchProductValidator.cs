using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.BranchProducts;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using FluentValidation;

namespace Application.Feature.Common.Validators.BranchProducts
{
	public class BranchProductValidator : AbstractValidator<BranchProductModel>
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IActionAccessorService accessorService;

		public BranchProductValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
		{
			this.unitOfWork = unitOfWork;
			this.accessorService = accessorService;
			ApplyRules();
		}

		private void ApplyRules()
		{
			RuleFor(x => x.Name)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<BranchProduct>()
						.Property(x => x.Name)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				)
				.MaximumLength(256)
				.WithState(x =>
					Messager
						.Create<BranchProduct>()
						.Property(x => x.Name)
						.Message(MessageType.MaximumLength)
						.Build()
				);
		}
	}
}
