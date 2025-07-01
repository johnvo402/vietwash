using Ardalis.GuardClauses;
using Domain.Aggregates.Funds.Enums;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Funds
{
    public class FundBehavior : BaseEntity
    {
        public object Name { get; set; } = default!;

        public FundType Type { get; set; } = default!;

        public bool Generate { get; set; } = false;

        public bool Automatic { get; set; } = false;

        public FundBehavior() { }

        public FundBehavior(object name, FundType type, bool generate = false, bool auto = false)
        {
            Name = Guard.Against.Null(name, nameof(Name));
            Type = Guard.Against.EnumOutOfRange(type, nameof(Type));
            Generate = generate;
            Automatic = auto;
        }

        //public ICollection<Fund> Funds { get; set; } = [];
    }
}
