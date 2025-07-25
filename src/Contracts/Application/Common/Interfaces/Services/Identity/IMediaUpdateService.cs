using Application.Common.Interfaces.Registers;
using Contracts.Dtos.Models;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces.Services.Identity;

public interface IMediaUpdateService : ISingleton
{
    string? GetKey(IFormFile? avatar, MediaType mediaType);

    Task<string?> UploadMediaAsync(IFormFile? avatar, string? key);
    Task<string?> UploadMultiPartMediaAsync(IFormFile? avatar, string? key);

    Task DeleteMediaAsync(string? key);
}
