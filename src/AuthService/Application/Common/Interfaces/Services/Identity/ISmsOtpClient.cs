
using Domain.Otp;

namespace Application.Common.Interfaces.Services.Identity
{
    public interface ISmsOtpClient
    {
        Task CreatePinAsync(
        CreatePinRequest request,
        CancellationToken cancellationToken = default
    );
        Task<bool> VerifyPinAsync(
            VerifyPinRequest request,
            CancellationToken cancellationToken = default
        );
    }
}
