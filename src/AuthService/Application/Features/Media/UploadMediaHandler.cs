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
            var listKey = new List<string?>();
            try
            {
                foreach (var file in request.Files)
                {
                    string? key = mediaUpdateService.GetKey(file, request.MediaType);
                    var path = await mediaUpdateService.UploadAvatarAsync(file, key);
                    listKey.Add(path);
                }
                return Result<UploadMediaResponse>.Success(
                    new UploadMediaResponse { Key = listKey }
                );
            }
            catch (System.Exception)
            {
                foreach (var key in listKey)
                {
                    await mediaUpdateService.DeleteAvatarAsync(key);
                }
                throw;
            }
        }
    }
}
