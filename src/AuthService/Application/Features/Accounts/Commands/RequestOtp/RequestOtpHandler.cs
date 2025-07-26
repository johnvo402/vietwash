using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Contracts.ApiWrapper;
using Domain.Otp;
using Mediator;

namespace Application.Features.Accounts.Commands.RequestOtp;

public class RequestOtpHandler(ISmsOtpClient _client, ICurrentAccount _currentAccount)
    : IRequestHandler<RequestOtpCommand, Result>
{
    public async ValueTask<Result> Handle(
        RequestOtpCommand request,
        CancellationToken cancellationToken
    )
    {
        var error = await _client.CreateAsync(
            new CreatePinRequest
            {
                To = request.To,
                ClientIp = _currentAccount.ClientIp!,
                Type = request.Type,
            },
            cancellationToken
        );
        if (error != null)
        {
            return Result.Failure(error);
        }

        return Result.Success();
    }
}
