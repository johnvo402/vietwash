using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Orders;
using Application.Feature.Common.Validators.Suppliers;
using Domain.Aggregates.Suppliers;
using FluentValidation;
using Infrastructure.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
