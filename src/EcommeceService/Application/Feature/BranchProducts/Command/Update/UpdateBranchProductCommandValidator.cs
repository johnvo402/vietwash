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

        public UpdateBranchProductCommandValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => x.BranchProduct).SetValidator(new BranchProductValidator(unitOfWork));
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
        }

        private async Task<bool> IsSkuExistsAsync(
            UpdateBranchProductCommand command,
            string sku,
            CancellationToken cancellation
        )
        {
            return !await unitOfWork
                .Repository<BranchProduct>()
                .AnyAsync(bp => bp.Sku == sku && bp.Id != command.BranchProductId, cancellation);
        }
    }
}
