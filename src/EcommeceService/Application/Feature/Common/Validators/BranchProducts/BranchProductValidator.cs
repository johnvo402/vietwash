using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.BranchProducts;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using Domain.Aggregates.Services;
using FluentValidation;

namespace Application.Feature.Common.Validators.BranchProducts
{
    public class BranchProductValidator : AbstractValidator<BranchProductModel>
    {
        private readonly IUnitOfWork _unitOfWork;

        public BranchProductValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
            RuleFor(x => x.CapitalPrice)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<BranchProduct>()
                        .Property(x => x.CapitalPrice)
                        .Message(MessageType.Empty)
                        .Negative()
                        .Build()
                )
                .LessThan(0)
                .WithState(x =>
                    Messager
                        .Create<BranchProduct>()
                        .Property(x => x.CapitalPrice)
                        .Message(MessageType.LessThan)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.CategoryId)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<BranchProduct>()
                        .Property(x => x.CategoryId)
                        .Message(MessageType.Empty)
                        .Negative()
                        .Build()
                )
                .MustAsync(IsCategoryExistsAsync)
                .WithState(x =>
                    Messager
                        .Create<BranchProduct>()
                        .Property(x => x.CategoryId)
                        .Message(MessageType.Existence)
                        .Negative()
                        .Build()
                );
        }

        private async Task<bool> IsCategoryExistsAsync(
            long categoryId,
            CancellationToken cancellation
        )
        {
            return await _unitOfWork
                .Repository<Category>()
                .AnyAsync(c => c.Id == categoryId, cancellation);
        }
    }
}
