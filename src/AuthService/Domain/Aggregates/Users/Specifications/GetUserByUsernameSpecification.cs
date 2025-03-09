using JohnChum.SharedKernel.Domain.Common.Specs;
using Microsoft.EntityFrameworkCore;

namespace Domain.Aggregates.Users.Specifications;

public class GetUserByUsernameSpecification : Specification<User>
{
    public GetUserByUsernameSpecification(string username)
    {
        Query.Where(x => EF.Functions.ILike(x.Username, username))
        .Include(x => x.Role)
        .ThenInclude(x => x!.RoleClaims!.Where(r => r.ClaimType == "permission"))
        .AsNoTracking();
    }
}
