using Ardalis.GuardClauses;
using Domain.Aggregates.Funds.Enums;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Funds
{
    public class FundBehavior : BaseEntity
    {
        public string Name { get; set; } = default!;

        public FundType Type { get; set; } = default!;

        public FundBehavior(string name, FundType type)
        {
            Name = Guard.Against.Null(name, nameof(Name));
            Type = Guard.Against.EnumOutOfRange(type, nameof(Type));
        }
        //public ICollection<Fund> Funds { get; set; } = [];
    }
}
