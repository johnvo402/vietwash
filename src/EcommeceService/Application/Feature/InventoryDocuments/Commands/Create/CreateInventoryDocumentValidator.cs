using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Inventories;
using FluentValidation;

namespace Application.Feature.InventoryDocuments.Commands.Create
{
    public class CreateInventoryDocumentValidator
        : AbstractValidator<CreateInventoryDocumentCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActionAccessorService _accessorService;

        public CreateInventoryDocumentValidator(
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
