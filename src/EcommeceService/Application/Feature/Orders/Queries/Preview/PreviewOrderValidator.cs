using FluentValidation;

namespace Application.Feature.Orders.Queries.Preview;

public sealed class PreviewOrderValidator : AbstractValidator<PreviewOrderQuery>
{
    public PreviewOrderValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.TariffId).GreaterThan(0);
        RuleFor(x => x.OrderItems).NotEmpty();
        RuleForEach(x => x.OrderItems)
            .NotNull()
            .ChildRules(item =>
            {
                item.RuleFor(x => x.ServiceId).GreaterThan(0);
                item.RuleFor(x => x.UnitRelationId).GreaterThan(0);
                item.RuleFor(x => x.Quantity).GreaterThan(0);
            });
    }
}
