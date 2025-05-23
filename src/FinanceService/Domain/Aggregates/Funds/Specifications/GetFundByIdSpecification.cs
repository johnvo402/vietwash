using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Funds.Specifications
{
    public class GetFundByIdSpecification : Specification<Fund>
    {
        public GetFundByIdSpecification(long id)
        {
            Query
                .Where(x => x.Id == id)
                .AsSplitQuery();
        }
    }
}
