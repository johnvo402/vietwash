using Domain.Aggregates.Accounts.Enums;
using Domain.Otp.Enums;

namespace Domain.Otp
{
    public class CreatePinRequest
    {
        public string To { get; set; } = string.Empty; // Phone number to send OTP to
        public string ClientIp { get; set; } = string.Empty; // Client IP for security checks
        public OtpType Type { get; set; } = OtpType.Phone; // Type of OTP (Phone or Email)
    }
}
