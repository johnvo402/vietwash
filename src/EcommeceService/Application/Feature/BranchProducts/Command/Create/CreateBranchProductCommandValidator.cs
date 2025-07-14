using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.BranchProducts;
using Application.Feature.Common.Validators.BranchProducts;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using FluentValidation;

namespace Application.Feature.BranchProducts.Command.Create
{
    public class CreateBranchProductCommandValidator : AbstractValidator<CreateBranchProductCommand>
    {
        private readonly IUnitOfWork unitOfWork;

        public CreateBranchProductCommandValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            ApplyRules();
        }

        private void ApplyRules()
        {
            Include(new BranchProductValidator(unitOfWork));
            RuleFor(x => x.Sku)
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

        private async Task<bool> IsSkuExistsAsync(string sku, CancellationToken cancellation)
        {
            return !await unitOfWork
                .Repository<BranchProduct>()
                .AnyAsync(p => p.Sku == sku, cancellation);
        }
    }
}
