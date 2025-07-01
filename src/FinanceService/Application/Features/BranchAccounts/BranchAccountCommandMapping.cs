using System.Linq.Expressions;
using Domain.Aggregates.Users;
using Specification;

namespace Application.Features.BranchAccounts
{
    public static class BranchAccountCommandMapping
    {
        public static Expression<Func<User, ListIds>> SelectOnlyId() =>
            user => new ListIds { Id = user.Id };
    }
}
