using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Suppliers;
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
            RuleFor(x => x.Supplier)
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
            RuleFor(x => x.Supplier.Email)
   .EmailAddress()
   .When(x => !string.IsNullOrEmpty(x.Supplier.Email))
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

            RuleFor(x => x.Supplier.Code)
                .NotEmpty()
                .WithState(x =>
                        Messager
                            .Create<Supplier>()
                            .Property(x => x.Code)
                            .Message(MessageType.Null)
                            .Negative()
                            .Build()
                )
                .MustAsync(CodeExists)
                .When(x => !string.IsNullOrEmpty(x.Supplier.Code))
                .WithState(x => Messager.Create<Supplier>()
                    .Property(x => x.Code)
                    .Message(MessageType.Existence)
                    .Negative()
                    .Build());
        }


        private async Task<bool> IsEmailUniqueAsync(UpdateSupplierCommand model, string email, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(email))
                return true;

            var existingSupplier = await _unitOfWork.Repository<Supplier>()
                .FindByConditionAsync(s => s.Email == email && s.Id != model.SupplierId, cancellationToken);

            return existingSupplier == null;
        }
        private async Task<bool> CodeExists(UpdateSupplierCommand model, string code, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(code))
                return true;

            var existingSupplier = await _unitOfWork.Repository<Supplier>()
                .FindByConditionAsync(s => s.Code == code && s.Id != model.SupplierId, cancellationToken);

            return existingSupplier == null;
        }
    }
	}

