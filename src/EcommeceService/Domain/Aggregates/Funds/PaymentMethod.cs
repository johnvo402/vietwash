using Domain.Aggregates.Orders;
using JohnChum.SharedKernel.Domain.Common;


namespace Domain.Aggregates.Funds
{
    public class PaymentMethod : DefaultEntity<string>
    {
        public string Name { get; set; } = default!;

        public ICollection<Fund> Funds { get; set; } = [];

    }
}
