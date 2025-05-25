using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Warehouses;
using FluentValidation;

namespace Application.Features.Warehouses.Commands.Create
{
    public partial class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
    {
        public CreateWarehouseCommandValidator(IUnitOfWork unitOfWork, IActionAccessorService actionAccessorService)
        {
            Include(new WarehouseValidator(unitOfWork, actionAccessorService));
        }
    }
}
