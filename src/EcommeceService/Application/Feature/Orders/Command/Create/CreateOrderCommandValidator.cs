using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Orders;
using Domain.Aggregates.Orders;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Command.Create
{
	public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IActionAccessorService _accessorService;

		public CreateOrderCommandValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
		{
			_unitOfWork = unitOfWork;
			_accessorService = accessorService;
			ApplyRules();
		}

		private void ApplyRules()
		{
			Include(new OrderValidator(_unitOfWork, _accessorService));
		}

	}
}
