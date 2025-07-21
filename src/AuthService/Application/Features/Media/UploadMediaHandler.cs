using Application.Common.Interfaces.Services.Identity;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Media
{
    public class UploadMediaHandler(IMediaUpdateService mediaUpdateService)
        : IRequestHandler<UploadMediaCommand, Result<UploadMediaResponse>>
    {
        private const long CheckFileSize = 5 * 1024 * 1024;

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
                    if (file.Length >= CheckFileSize)
                    {
                        await mediaUpdateService.UploadMultiPartMediaAsync(file, key);
                    }
                    else
                    {
                        await mediaUpdateService.UploadMediaAsync(file, key);
                    }
                    listKey.Add(key);
                }
                return Result<UploadMediaResponse>.Success(
                    new UploadMediaResponse { Key = listKey }
                );
            }
            catch (Exception)
            {
                foreach (var key in listKey)
                {
                    await mediaUpdateService.DeleteMediaAsync(key);
                }
                throw;
            }
        }
    }
}
