using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Otp;
using Domain.Otp.Enums;
using Mediator;

namespace Application.Features.Accounts.Commands.VerifyOtpNormal
{
    public class VerifyOtpNormalHandler(ICurrentAccount _currentAccount, ISmsOtpClient _client)
        : IRequestHandler<VerifyOtpNormalCommand, Result>
    {
        public async ValueTask<Result> Handle(
            VerifyOtpNormalCommand request,
            CancellationToken cancellationToken
        )
        {
            var verifyRequest = new VerifyPinRequest
            {
                Otp = request.Otp,
                ClientIp = _currentAccount.ClientIp!,
            };

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                verifyRequest.To = request.PhoneNumber;
                verifyRequest.Type = OtpType.Phone;
            }
            else if (!string.IsNullOrEmpty(request.Email))
            {
                verifyRequest.To = request.Email;
                verifyRequest.Type = OtpType.Email;
            }

            // Verify OTP
            bool isValid = await _client.VerifyAsync(verifyRequest, cancellationToken);
            if (isValid)
            {
                return Result.Success();
            }
            else
            {
                return Result.Failure(
                    new BadRequestError(
                        "Invalid OTP",
                        Messager
                            .Create<VerifyPinRequest>()
                            .Property(x => x.Otp!)
                            .Message(MessageType.Valid)
                            .Negative()
                            .Build()
                    )
                );
            }
        }
    }
}
