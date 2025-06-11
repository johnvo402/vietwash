using Domain.Aggregates.Accounts.Enums;

namespace Domain.Otp
{
    public class VerifyPinRequest
    {
        public long? AccountId { get; set; }
        public string? Key { get; set; }
        public string Otp { get; set; } = default!;
        public AccountActivityType Type { get; set; } = default!;
    }
}
