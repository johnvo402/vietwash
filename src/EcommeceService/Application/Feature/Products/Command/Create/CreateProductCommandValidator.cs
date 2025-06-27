using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Products;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using FluentValidation;


namespace Application.Feature.Products.Command.Create
{
	public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
	{
		private readonly IUnitOfWork _unitOfWork;

		private readonly IActionAccessorService _accessorService;

		public CreateProductCommandValidator(
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
			Include(new ProductValidator(_unitOfWork, _accessorService));
			RuleFor(x => x.Sku)
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
			RuleFor(x => x.Barcode)
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
		private async Task<bool> IsSkuExistsAsync(string sku, CancellationToken cancellation)
		{
			return !await _unitOfWork.Repository<Product>().AnyAsync(p => p.Sku == sku, cancellation);
		}
		private async Task<bool> IsBarcodeExistsAsync(string barcode, CancellationToken cancellation)
		{
			return !await _unitOfWork.Repository<Product>().AnyAsync(p => p.Barcode == barcode, cancellation);
		}
	}
}
