using Application.Common.Interfaces.Registers;
using Contracts.Dtos.Models;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces.Services.Identity;

public interface IMediaUpdateService : ISingleton
{
    string? GetKey(IFormFile? avatar, MediaType mediaType);

    Task<string?> UploadAvatarAsync(IFormFile? avatar, string? key);

    Task DeleteAvatarAsync(string? key);
}
