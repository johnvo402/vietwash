using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.InventoryDocuments;
using Application.Feature.Common.Validators.Suppliers;
using Application.Feature.Suppliers.Command.Update;
using Domain.Aggregates.Inventories;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.InventoryImports.Command.Update
{
    public class UpdateInventoryImportCommandValidator : AbstractValidator<UpdateInventoryImportCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IActionAccessorService _accessorService;

        public UpdateInventoryImportCommandValidator(
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
            RuleFor(x => x.Body.InventoryImportModel)
                .SetValidator(new InventoryDocumentValidator(_unitOfWork, _accessorService));
            RuleFor(x => x.InventoryImportId)
                .NotEmpty()
                .WithState(x =>
                        Messager
                            .Create<InventoryDocument>()
                            .Property(x => x.Id)
                            .Message(MessageType.Null)
                            .Negative()
                            .Build()
                );
        }
    }
}
