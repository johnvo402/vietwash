using System.Linq.Expressions;
using Domain.Aggregates.Accounts;
using Specification;

namespace Application.Features.BranchAccounts
{
    public static class BranchAccountCommandMapping
    {
        public static Expression<Func<Account, ListIds>> SelectOnlyId() =>
            user => new ListIds { Id = user.Id };
    }
}
