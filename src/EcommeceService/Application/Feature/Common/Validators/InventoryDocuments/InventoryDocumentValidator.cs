using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.InventoryImports;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;

namespace Application.Feature.Common.Validators.InventoryDocuments
{
    public class InventoryDocumentValidator : AbstractValidator<InventoryImportModel>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IActionAccessorService accessorService;

        public InventoryDocumentValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
        {
            this.unitOfWork = unitOfWork;
            this.accessorService = accessorService;
            ApplyRules();
        }
        private void ApplyRules()
        {
            RuleFor(x => x.PaidAmount)
                .GreaterThanOrEqualTo(0)
                .WithState(x =>
                    Messager
                        .Create<InventoryImportModel>()
                        .Property(x => x.PaidAmount)
                        .Message(MessageType.GreaterThanEqual)
                        .Negative()
                        .Build()
                );
            RuleFor(x => x.ProductItems)
                .ForEach(item =>
                {
                    item.SetValidator(new ProductImportItemValidator(unitOfWork, accessorService));
                });
            RuleFor(x => x.EquipmentItems)
                .ForEach(item =>
                {
                    item.SetValidator(new EquipmentImportItemValidator(unitOfWork, accessorService));
                });
        }
    }
}
