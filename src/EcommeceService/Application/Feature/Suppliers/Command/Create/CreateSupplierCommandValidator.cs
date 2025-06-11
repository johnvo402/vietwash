using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Suppliers;
using Application.Feature.Common.Validators.Suppliers;
using Domain.Aggregates.Suppliers;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;


namespace Application.Feature.Suppliers.Command.Create
{
    public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IActionAccessorService _accessorService;

        public CreateSupplierCommandValidator(
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
            Include(new SupplierValidator(_unitOfWork, _accessorService));
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

            RuleFor(x => x.Code)
                .MustAsync(CodeExists)
                .When(x => !string.IsNullOrEmpty(x.Code))
                .WithState(x => Messager.Create<Supplier>()
                    .Property(x => x.Code)
                    .Message(MessageType.Existence)
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
        private async Task<bool> CodeExists(string code, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(code))
                return true;

            var existingSupplier = await _unitOfWork.Repository<Supplier>()
                .FindByConditionAsync(s => s.Code == code, cancellationToken);

            return existingSupplier == null;
        }
    }
}
