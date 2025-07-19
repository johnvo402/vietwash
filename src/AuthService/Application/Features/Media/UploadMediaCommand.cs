using Contracts.ApiWrapper;
using Contracts.Dtos.Models;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Media
{
    public class UploadMediaCommand : IRequest<Result<UploadMediaResponse>>
    {
        public IList<IFormFile> Files { get; set; } = default!;
        public MediaType MediaType { get; set; }
    }
}
