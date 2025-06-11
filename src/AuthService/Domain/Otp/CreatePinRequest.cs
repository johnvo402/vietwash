using Domain.Aggregates.Accounts.Enums;

namespace Domain.Otp
{
    public class CreatePinRequest
    {
        public long? AccountId { get; set; }
        public string To { get; set; } = default!;
        public AccountActivityType Type { get; set; } = default!;
    }
}
