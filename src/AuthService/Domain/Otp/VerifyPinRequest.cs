using Domain.Aggregates.Accounts.Enums;

namespace Domain.Otp
{
    public class VerifyPinRequest
    {
        public string To { get; set; } = string.Empty; // Phone number associated with OTP
        public string Otp { get; set; } = string.Empty; // OTP code entered by user
        public string ClientIp { get; set; } = string.Empty; // Client IP for security checks
    }
}
