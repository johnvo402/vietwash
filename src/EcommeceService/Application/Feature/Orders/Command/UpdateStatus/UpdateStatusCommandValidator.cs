using Domain.Aggregates.Orders.Enums;
using FluentValidation;

namespace Application.Feature.Orders.Command.UpdateStatus;

public class UpdateStatusCommandValidator : AbstractValidator<UpdateStatusCommand>
{
    public UpdateStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .Must(value => long.TryParse(value, out long id) && id > 0)
            .WithMessage("OrderId must be a positive integer.");
        RuleFor(x => x.Model).NotNull();

        When(
            x => x.Model is not null,
            () =>
            {
                RuleFor(x => x.Model.Status).NotNull().IsInEnum();
                RuleFor(x => x.Model.PaymentMethod)
                    .NotNull()
                    .IsInEnum()
                    .When(x => x.Model.Status == OrderStatus.Completed);
                RuleFor(x => x.Model.PaymentMethod)
                    .Null()
                    .When(x => x.Model.Status != OrderStatus.Completed);
                RuleFor(x => x.Model.OrderEquipments)
                    .Must(items =>
                        items is null
                        || items.Select(item => item.EquipmentId).Distinct().Count() == items.Count
                    )
                    .WithMessage("Duplicate equipment ids are not allowed.");
                RuleFor(x => x.Model.OrderEquipments)
                    .Empty()
                    .When(x => x.Model.Status != OrderStatus.InProgress);
                RuleForEach(x => x.Model.OrderEquipments)
                    .ChildRules(item => item.RuleFor(x => x.EquipmentId).GreaterThan(0));
            }
        );
    }
}
