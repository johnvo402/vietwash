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


			RuleFor(x => x.Phone)
				.Matches(@"^\+?[0-9]{7,15}$") // cho phép định dạng: +84xxx, 0123..., không chứa ký tự đặc biệt
				.When(x => !string.IsNullOrWhiteSpace(x.Phone))
				.WithState(x => Messager.Create<Supplier>()
					.Property(x => x.Phone)
					.Message(MessageType.Valid)
					.Negative()
					.Build());
		}

    }
}
