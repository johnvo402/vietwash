using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections.Warehouses;
using Domain.Aggregates.Branches;
using Domain.Aggregates.Warehouses;
using Domain.Aggregates.Warehouses.Enums;
using FluentValidation;
using Infrastructure.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;

namespace Application.Features.Common.Validators.Warehouses
{
    public class WarehouseValidator : AbstractValidator<WarehouseModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActionAccessorService _actionAccessorService;

        public WarehouseValidator(IUnitOfWork unitOfWork, IActionAccessorService actionAccessorService)
        {
            _unitOfWork = unitOfWork;
            _actionAccessorService = actionAccessorService;
            ApplyRules();
        }
        private void ApplyRules()
        {
            RuleFor(x => x.Name)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<Warehouse>()
                    .Property(x => x.Name)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .MaximumLength(256)
            .WithState(x =>
                Messager
                    .Create<Warehouse>()
                    .Property(x => x.Name)
                    .Message(MessageType.MaximumLength)
                    .Build()
            );

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<Warehouse>()
                        .Property(x => x.Code)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .MaximumLength(100)
                .WithState(x =>
                    Messager
                        .Create<Warehouse>()
                        .Property(x => x.Code)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<Warehouse>()
                        .Property(x => x.Code)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .MaximumLength(500)
                .WithState(x =>
                    Messager
                        .Create<Warehouse>()
                        .Property(x => x.Description)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

           
            RuleFor(x => x.Status)
                .Must(status => Enum.IsDefined(typeof(WarehouseStatus), (WarehouseStatus)status))
                .WithState(x =>
                    Messager
                        .Create<Warehouse>()
                        .Property(x => x.Status)
                        .Message(MessageType.Existence)
                        .Build()
                );
        }
        private async Task<bool> IsCodeAvaiableAsync(string code, CancellationToken cancellationToken)
        => !await _unitOfWork.Repository<Warehouse>().AnyAsync(c => c.Code == code, cancellationToken);

        private async Task<bool> IsBranchIdAvaiableAsync(long branchId, CancellationToken cancellationToken)
        => await _unitOfWork.Repository<Warehouse>().AnyAsync(c => c.BranchId == branchId, cancellationToken);
    }
}
