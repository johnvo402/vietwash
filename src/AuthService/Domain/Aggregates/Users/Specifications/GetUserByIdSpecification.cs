using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Users.Specifications;

public class GetUserByIdSpecification : Specification<User>
{
    public GetUserByIdSpecification(long id)
    {
        Query
            .Where(x => x.Id == id)
            .AsSplitQuery();
    }
}
