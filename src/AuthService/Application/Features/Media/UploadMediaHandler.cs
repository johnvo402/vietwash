using Application.Common.Interfaces.Services.Identity;
using Infrastructure.Services.Identity;
using Mediator;

namespace Application.Features.Media
{
    public class UploadMediaHandler(IMediaUpdateService<Image> mediaUpdateService) : IRequestHandler<UploadMediaCommand, UploadMediaResponse>
    {
        public async ValueTask<UploadMediaResponse> Handle(UploadMediaCommand request, CancellationToken cancellationToken)
        {
            string? key = mediaUpdateService.GetKey(request.File);
            var path = await mediaUpdateService.UploadAvatarAsync(request.File, key);
            return new UploadMediaResponse
            {
                Key = path
            };
        }
    }
}
