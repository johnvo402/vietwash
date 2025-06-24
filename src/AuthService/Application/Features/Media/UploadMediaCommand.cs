using Contracts.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Media
{
    public class UploadMediaCommand : IRequest<Result<UploadMediaResponse>>
    {
        public IFormFile File { get; set; } = default!;
    }
}
