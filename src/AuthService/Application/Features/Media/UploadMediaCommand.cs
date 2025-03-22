

using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Media
{
    public class UploadMediaCommand : IRequest<UploadMediaResponse>
    {
        public IFormFile File { get; set; } = default!;
    }
}
