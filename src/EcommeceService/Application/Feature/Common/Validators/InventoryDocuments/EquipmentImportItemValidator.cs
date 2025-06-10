using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.InventoryImports;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Suppliers;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Validators.InventoryDocuments
{
    public class EquipmentImportItemValidator : AbstractValidator<EquipmentImportItem>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IActionAccessorService accessorService;

        public EquipmentImportItemValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
        {
            this.unitOfWork = unitOfWork;
            this.accessorService = accessorService;
            ApplyRules();
        }
        private void ApplyRules()
        {
            RuleFor(x => x.SupplierId)
                .MustAsync(IsSupplierExistsAsync)
                .WithState(x => Messager
                    .Create<Supplier>()
                    .Property(x => x.Id)
                    .Message(MessageType.Existence)
                    .Negative()
                    .Build());

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                    .WithState(x => Messager
                        .Create<EquipmentImportItem>()
                        .Property(x => x.Price)
                        .Message(MessageType.GreaterThanEqual)
                        .Negative()
                        .Build());

            RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0)
                .WithState(x => Messager
                    .Create<EquipmentImportItem>()
                    .Property(x => x.Discount)
                    .Message(MessageType.GreaterThanEqual)
                    .Negative()
                    .Build());
            RuleFor(x => x.Sku)
                .NotEmpty()
                .WithState(x => Messager
                    .Create<ProductImportItem>()
                    .Property(x => x.Sku)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build());

        }

        private async Task<bool> IsSupplierExistsAsync(long supplierId, CancellationToken cancellation)
        {
            return await unitOfWork.Repository<Supplier>().AnyAsync(s => s.Id == supplierId, cancellation);
        }
    }
}
