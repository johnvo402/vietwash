using FluentValidation;

namespace Application.Feature.Orders.Command.Create
{
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0);
            RuleFor(x => x.BranchId).GreaterThan(0);
            RuleFor(x => x.TariffId).GreaterThan(0);
            RuleFor(x => x.Note).MaximumLength(500);
            RuleFor(x => x.OrderItems).NotEmpty();
            RuleFor(x => x.OrderItems)
                .Must(items =>
                    items is null
                    || items.Select(x => (x.ServiceId, x.UnitRelationId)).Distinct().Count()
                        == items.Count
                )
                .WithMessage("Duplicate service and unit relation combinations are not allowed.");

            RuleForEach(x => x.OrderItems)
                .ChildRules(item =>
                {
                    item.RuleFor(x => x.ServiceId).GreaterThan(0);
                    item.RuleFor(x => x.UnitRelationId).GreaterThan(0);
                    item.RuleFor(x => x.Quantity).GreaterThan(0);
                });
        }
    }
}
