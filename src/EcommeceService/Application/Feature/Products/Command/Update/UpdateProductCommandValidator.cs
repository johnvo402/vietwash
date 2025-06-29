using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Products;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using FluentValidation;

namespace Application.Feature.Products.Command.Update
{
	public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IActionAccessorService accessorService;

		public UpdateProductCommandValidator(
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
			RuleFor(x => x.Product)
				.SetValidator(new ProductValidator(unitOfWork, accessorService));
			RuleFor(x => x.ProductId)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<Product>()
						.Property(x => x.Id)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				);
		}
	}
}
