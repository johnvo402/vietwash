using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.BranchProducts;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using Domain.Aggregates.Services;
using FluentValidation;


namespace Application.Feature.BranchProducts.Command.Update
{
	public class UpdateBranchProductCommandValidator : AbstractValidator<UpdateBranchProductCommand>
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IActionAccessorService accessorService;

		public UpdateBranchProductCommandValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
		{
			this.unitOfWork = unitOfWork;
			this.accessorService = accessorService;
			ApplyRules();
		}

		private void ApplyRules()
		{
			RuleFor(x => x.BranchProduct)
				.SetValidator(new UpdateBranchProductValidator(unitOfWork, accessorService));
			RuleFor(x => x.BranchProduct.Sku)
					.NotEmpty()
					.WithState(x =>
							Messager
								.Create<BranchProduct>()
								.Property(x => x.Sku)
								.Message(MessageType.Null)
								.Negative()
								.Build()
					)
					.MustAsync(IsSkuExistsAsync)
					.WithState(_ =>
							Messager
								.Create<BranchProduct>()
								.Property(x => x.Sku)
								.Message(MessageType.Found)
								.Negative()
								.Build()
					);
			RuleFor(x => x.BranchProduct.Barcode)
					.NotEmpty()
					.WithState(x =>
							Messager
								.Create<BranchProduct>()
								.Property(x => x.Barcode)
								.Message(MessageType.Null)
								.Negative()
								.Build()
					)
					.MustAsync(IsBarcodeExistsAsync)
					.WithState(_ =>
							Messager
								.Create<BranchProduct>()
								.Property(x => x.Barcode)
								.Message(MessageType.Found)
								.Negative()
								.Build()
					);
			RuleForEach(x => x.BranchProduct.UnitRelations)
					.MustAsync(async (command, unit, cancellationToken) =>
						unit.Id == 0 // UnitRelation mới thì bỏ qua
						|| await IsUnitRelationBelongToBranchProductAsync(command, unit.Id, cancellationToken)
					)
					.WithState(_ =>
						Messager.Create<UnitRelation>()
							.Property(x => x.Id)
							.Message(MessageType.Found)
							.Negative()
							.Build()
					);
		}
		private async Task<bool> IsSkuExistsAsync(UpdateBranchProductCommand command, string sku, CancellationToken cancellation)
		{
			return !await unitOfWork.Repository<BranchProduct>().AnyAsync(bp => bp.Sku == sku && bp.Id != command.BranchProductId, cancellation);
		}
		private async Task<bool> IsBarcodeExistsAsync(UpdateBranchProductCommand command, string barcode, CancellationToken cancellation)
		{
			return !await unitOfWork.Repository<BranchProduct>().AnyAsync(bp => bp.Barcode == barcode && bp.Id != command.BranchProductId, cancellation);
		}
		private async Task<bool> IsUnitRelationBelongToBranchProductAsync(UpdateBranchProductCommand command, long unitRelationId, CancellationToken cancellation)
		{
			return await unitOfWork.Repository<UnitRelation>()
				.AnyAsync(x => x.Id == unitRelationId && x.BranchProductId == command.BranchProductId, cancellation);
		}

	}
}
