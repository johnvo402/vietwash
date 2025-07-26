using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Accounts.Commands.VerifyOtpNormal;

public class VerifyOtpNormalCommand : IRequest<Result>
{
    public string PhoneNumber { get; set; } = string.Empty; // Phone number associated with OTP
    public string Email { get; set; } = string.Empty; // Phone number associated with OTP
    public string Otp { get; set; } = string.Empty; // OTP code entered by user
};
