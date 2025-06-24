using Contracts.ApiWrapper;
using Domain.Otp;
using Mediator;

namespace Application.Features.Accounts.Commands.VerifyOtpLoginCustomer;

public class VerifyOtpCommand : IRequest<Result<VerifyOtpResponse>>
{
    public string PhoneNumber { get; set; } = string.Empty; // Phone number associated with OTP
    public string Otp { get; set; } = string.Empty; // OTP code entered by user
};
