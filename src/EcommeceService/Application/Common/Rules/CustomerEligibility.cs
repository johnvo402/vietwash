using System.Linq.Expressions;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Users;
using Infrastructure.Constants;

namespace Application.Common.Rules;

/// <summary>The authoritative, SQL-translatable rule for an Order/voucher target customer.</summary>
public static class CustomerEligibility
{
    public static Expression<Func<User, bool>> ForId(long customerId) =>
        user =>
            user.Id == customerId
            && user.Role == ROLE.CUSTOMER
            && user.Status == ActivationStatus.Active
            && !user.Disabled;
}
