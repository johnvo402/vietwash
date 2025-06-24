using Domain.Aggregates.Accounts.Enums;

namespace Domain.Otp
{
    public class CreatePinRequest
    {
        public string To { get; set; } = string.Empty; // Phone number to send OTP to
        public string ClientIp { get; set; } = string.Empty; // Client IP for security checks
    }
}
