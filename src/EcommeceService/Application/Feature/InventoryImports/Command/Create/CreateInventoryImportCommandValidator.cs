using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.InventoryDocuments;
using Application.Feature.Common.Validators.Services;
using Application.Feature.Services.Command.Create;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.InventoryImports.Command.Create
{
    public class CreateInventoryImportCommandValidator : AbstractValidator<CreateInventoryImportCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IActionAccessorService _accessorService;

        public CreateInventoryImportCommandValidator(
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
            Include(new InventoryDocumentValidator(_unitOfWork, _accessorService));
        }
    }
}
