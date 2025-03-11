using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Funds
{
    public class FundType : DefaultEntity<string>
    {
        public string Name { get; set; } = default!;

    }
}
