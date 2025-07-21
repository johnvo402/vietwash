using Application.Common.Interfaces.Services.Identity;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Media
{
    public class UploadMediaHandler(IMediaUpdateService mediaUpdateService)
        : IRequestHandler<UploadMediaCommand, Result<UploadMediaResponse>>
    {
        public async ValueTask<Result<UploadMediaResponse>> Handle(
            UploadMediaCommand request,
            CancellationToken cancellationToken
        )
        {
            string? key = mediaUpdateService.GetKey(request.File, request.MediaType);
            var path = await mediaUpdateService.UploadAvatarAsync(request.File, key);
            return Result<UploadMediaResponse>.Success(new UploadMediaResponse { Key = path });
        }
    }
}
