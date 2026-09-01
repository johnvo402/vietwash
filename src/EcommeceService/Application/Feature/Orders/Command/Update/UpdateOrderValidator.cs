using FluentValidation;

namespace Application.Feature.Orders.Command.Update
{
    public class UpdateOrderValidator : AbstractValidator<UpdateOrderCommand>
    {
        public UpdateOrderValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0);
            RuleFor(x => x.Model).NotNull();
            When(
                x => x.Model is not null,
                () =>
                {
                    RuleFor(x => x.Model.TariffId).GreaterThan(0);
                    RuleFor(x => x.Model.Note).MaximumLength(500);
                    RuleFor(x => x.Model.OrderItems).NotEmpty();
                    RuleFor(x => x.Model.OrderItems)
                        .Must(items =>
                            items is null
                            || items
                                    .Select(x => (x.ServiceId, x.UnitRelationId))
                                    .Distinct()
                                    .Count() == items.Count
                        )
                        .WithMessage(
                            "Duplicate service and unit relation combinations are not allowed."
                        );

                    RuleForEach(x => x.Model.OrderItems)
                        .ChildRules(item =>
                        {
                            item.RuleFor(x => x.ServiceId).GreaterThan(0);
                            item.RuleFor(x => x.UnitRelationId).GreaterThan(0);
                            item.RuleFor(x => x.Quantity).GreaterThan(0);
                        });
                }
            );
        }
    }
}
