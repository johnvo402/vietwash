using Domain.Aggregates.Funds.Enums;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Funds
{
    public class FundBehavior : BaseEntity
    {
        public string Name { get; set; } = default!;

        public FundType Type { get; set; } = default!;

        //public ICollection<Fund> Funds { get; set; } = [];
    }
}
