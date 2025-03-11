using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Funds
{
    public class FundBehavior : DefaultEntity<string>
    {
        public string Name { get; set; } = default!;
    }
}
