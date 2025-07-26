using Contracts.ApiWrapper;
using Domain.Otp;
using Mediator;

namespace Application.Features.Accounts.Commands.RequestOtp;

public class RequestOtpCommand : CreatePinRequest, IRequest<Result>;
