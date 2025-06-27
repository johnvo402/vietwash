using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Products;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using FluentValidation;
using Infrastructure.UnitOfWorks;

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
			RuleFor(x => x.Product.Sku)
					.NotEmpty()
					.WithState(x =>
							Messager
								.Create<Product>()
								.Property(x => x.Sku)
								.Message(MessageType.Null)
								.Negative()
								.Build()
					)
					.MustAsync(IsSkuExistsAsync)
					.WithState(_ =>
							Messager
								.Create<Product>()
								.Property(x => x.Sku)
								.Message(MessageType.Found)
								.Negative()
								.Build()
					);
			RuleFor(x => x.Product.Barcode)
					.NotEmpty()
					.WithState(x =>
							Messager
								.Create<Product>()
								.Property(x => x.Barcode)
								.Message(MessageType.Null)
								.Negative()
								.Build()
					)
					.MustAsync(IsBarcodeExistsAsync)
					.WithState(_ =>
							Messager
								.Create<Product>()
								.Property(x => x.Barcode)
								.Message(MessageType.Found)
								.Negative()
								.Build()
					);
		}
		private async Task<bool> IsSkuExistsAsync(UpdateProductCommand command, string sku, CancellationToken cancellation)
		{
			return !await unitOfWork.Repository<Product>().AnyAsync(p => p.Sku == sku && p.Id != command.ProductId, cancellation);
		}
		private async Task<bool> IsBarcodeExistsAsync(UpdateProductCommand command, string barcode, CancellationToken cancellation)
		{
			return !await unitOfWork.Repository<Product>().AnyAsync(p => p.Barcode == barcode && p.Id != command.ProductId, cancellation);
		}
	}
}
