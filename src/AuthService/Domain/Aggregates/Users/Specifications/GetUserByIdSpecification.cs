using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Users.Specifications;

public class GetUserByIdSpecification : Specification<User>
{
    public GetUserByIdSpecification(Ulid id)
    {
        Query
            .Where(x => x.Id == id)
            .Include(x => x.Role)
            .ThenInclude(x => x!.RolePermissions)!.ThenInclude(x => x!.Permission)
            .Include(x => x.Address!.Province)
            .Include(x => x.Address!.District)
            .Include(x => x.Address!.Commune)
            .AsSplitQuery();
    }
}
