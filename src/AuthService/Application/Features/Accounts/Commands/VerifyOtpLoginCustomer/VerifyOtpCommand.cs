using Domain.Otp;
using Mediator;

namespace Application.Features.Accounts.Commands.VerifyOtpLoginCustomer;

public class VerifyOtpCommand : VerifyPinRequest, IRequest<VerifyOtpResponse>
{
    public string PhoneNumber { get; set; } = default!;
};
