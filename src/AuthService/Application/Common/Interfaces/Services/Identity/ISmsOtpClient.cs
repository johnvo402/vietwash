using Contracts.ApiWrapper;
using Domain.Otp;

namespace Application.Common.Interfaces.Services.Identity
{
    public interface ISmsOtpClient
    {
        Task<ErrorDetails?> CreateAsync(
            CreatePinRequest request,
            CancellationToken cancellationToken = default
        );
        Task<bool> VerifyAsync(
            VerifyPinRequest request,
            CancellationToken cancellationToken = default
        );
    }
}
