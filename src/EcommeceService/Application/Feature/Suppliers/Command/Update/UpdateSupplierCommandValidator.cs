using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Suppliers;
using Domain.Aggregates.Suppliers;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;


namespace Application.Feature.Suppliers.Command.Update
{
    public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IActionAccessorService _accessorService;

        public UpdateSupplierCommandValidator(
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
            RuleFor(x => x.Body.Supplier)
                .SetValidator(new SupplierValidator(_unitOfWork, _accessorService));
            RuleFor(x => x.SupplierId)
                .NotEmpty()
                .WithState(x =>
                        Messager
                            .Create<Supplier>()
                            .Property(x => x.Id)
                            .Message(MessageType.Null)
                            .Negative()
                            .Build()
                );
        }
    }
}
