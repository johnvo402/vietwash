using JohnChum.SharedKernel.Domain.Common.Specs;


namespace Domain.Aggregates.Roles.Specifications
{
    public class GetPermissionSpecification : Specification<Permission>
    {
        public GetPermissionSpecification()
        {
            Query
            .AsNoTracking()
            .AsSplitQuery();
            string key = GetUniqueCachedKey();
            Query.EnableCache(key);
        }
    }
}
