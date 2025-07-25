using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.EquipmentActivities;
using Contracts.Common.Messages;
using Domain.Aggregates.Equipments;
using FluentValidation;

namespace Application.Feature.Common.Validators.EquipmentActivities
{
    public class EquipmentActivityValidator : AbstractValidator<EquipmentActivityModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActionAccessorService _accessorService;

        public EquipmentActivityValidator(
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
            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithState(x =>
                    Messager
                        .Create<EquipmentActivityModel>(nameof(EquipmentActivity))
                        .Property(x => x.Description)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );
            RuleFor(x => x.LaborCost)
                .GreaterThanOrEqualTo(0)
                .WithState(x =>
                    Messager
                        .Create<EquipmentActivityModel>(nameof(EquipmentActivity))
                        .Property(x => x.LaborCost)
                        .Message(MessageType.GreaterThanEqual)
                        .Negative()
                        .Build()
                );
            RuleForEach(x => x.Details)
                .ChildRules(detail =>
                {
                    detail
                        .RuleFor(x => x.PartName)
                        .NotEmpty()
                        .WithState(x =>
                            Messager
                                .Create<EquipmentActivityDetailModel>(
                                    nameof(EquipmentActivityDetail)
                                )
                                .Property(x => x.PartName)
                                .Message(MessageType.Null)
                                .Negative()
                                .Build()
                        )
                        .MaximumLength(256)
                        .WithState(x =>
                            Messager
                                .Create<EquipmentActivityDetailModel>(
                                    nameof(EquipmentActivityDetail)
                                )
                                .Property(x => x.PartName)
                                .Message(MessageType.MaximumLength)
                                .Build()
                        );
                    detail
                        .RuleFor(x => x.Quantity)
                        .GreaterThan(0)
                        .WithState(x =>
                            Messager
                                .Create<EquipmentActivityDetailModel>(
                                    nameof(EquipmentActivityDetail)
                                )
                                .Property(x => x.Quantity)
                                .Message(MessageType.GreaterThan)
                                .Negative()
                                .Build()
                        );
                    detail
                        .RuleFor(x => x.UnitPrice)
                        .GreaterThanOrEqualTo(0)
                        .WithState(x =>
                            Messager
                                .Create<EquipmentActivityDetailModel>(
                                    nameof(EquipmentActivityDetail)
                                )
                                .Property(x => x.UnitPrice)
                                .Message(MessageType.GreaterThanEqual)
                                .Negative()
                                .Build()
                        );
                });
        }
    }
}
