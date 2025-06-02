using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Services.Command.Delete;
using Domain.Aggregates.Suppliers;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Suppliers.Command.Delete
{
	public class DeleteSupplierCommandValidator : AbstractValidator<DeleteSupplierCommand>
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IActionAccessorService accessorService;

		public DeleteSupplierCommandValidator(
			IUnitOfWork unitOfWork,
			IActionAccessorService accessorService
		)
		{
			this.unitOfWork = unitOfWork;
			this.accessorService = accessorService;
			ApplyRules();
		}
		private void ApplyRules()
		{
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
