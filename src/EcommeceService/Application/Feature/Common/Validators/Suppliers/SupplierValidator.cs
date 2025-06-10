using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Suppliers;
using Domain.Aggregates.Suppliers;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
namespace Application.Feature.Common.Validators.Suppliers
{
    internal class SupplierValidator : AbstractValidator<SupplierModel>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IActionAccessorService _accessorService;

        public SupplierValidator(
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
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithState(x => Messager.Create<Supplier>()
                    .Property(x => x.Name)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build())
                .MaximumLength(256)
                .WithState(x => Messager.Create<Supplier>()
                    .Property(x => x.Name)
                    .Message(MessageType.MaximumLength)
                    .Build());
            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.Email))
                .WithState(x => Messager.Create<Supplier>()
                    .Property(x => x.Email)
                    .Message(MessageType.Valid)
                    .Negative()
                    .Build())
                .MustAsync(IsEmailUniqueAsync)
                .WithState(x => Messager.Create<Supplier>()
                    .Property(x => x.Email)
                    .Message(MessageType.Existence)
                    .Negative()
                    .Build());
            RuleFor(x => x.Phone)
                .Matches(@"^\+?[0-9]{7,15}$") // cho phép định dạng: +84xxx, 0123..., không chứa ký tự đặc biệt
                .When(x => !string.IsNullOrWhiteSpace(x.Phone))
                .WithState(x => Messager.Create<Supplier>()
                    .Property(x => x.Phone)
                    .Message(MessageType.Valid)
                    .Negative()
                    .Build());
        }
        private async Task<bool> IsEmailUniqueAsync(SupplierModel model, string email, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(email))
                return true;

            var existingSupplier = await _unitOfWork.Repository<Supplier>()
                .FindByConditionAsync(s => s.Email == email, cancellationToken);

            return existingSupplier == null;
        }
    }
}
