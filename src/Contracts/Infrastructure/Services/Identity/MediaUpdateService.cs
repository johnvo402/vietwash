using Application.Common.Interfaces.Services.Aws;
using Application.Common.Interfaces.Services.Identity;
using Contracts.Dtos.Models;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Infrastructure.Services.Identity;

public class MediaUpdateService(IAmazonS3Service awsAmazonService, ILogger logger)
    : IMediaUpdateService
{
    public async Task DeleteMediaAsync(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        var response = await awsAmazonService.DeleteAsync(key);

        if (!response.IsSuccess)
        {
            logger.Information("Remove object {key} fail with error: {error}", key, response.Error);
            return;
        }

        logger.Information("Remove object {key} successfully.", key);
    }

    public string? GetKey(IFormFile? avatar, MediaType mediaType)
    {
        if (avatar == null)
        {
            return null;
        }

        return $"{mediaType}s/{awsAmazonService.UniqueFileName(avatar.FileName)}";
    }

    public async Task<string?> UploadMediaAsync(IFormFile? avatar, string? key)
    {
        if (avatar == null || string.IsNullOrEmpty(key))
        {
            return null;
        }

        AwsResponse response = await awsAmazonService.UploadAsync(avatar!.OpenReadStream(), key);

        if (!response.IsSuccess)
        {
            logger.Information(
                "\nUpdate media has had error with file upload: {error}.\n",
                response.Error
            );
            return null;
        }

        logger.Information(
            "\nUpdate media success full with the path: {path}.\n",
            response.S3UploadedPath
        );
        return key;
    }

    public async Task<string?> UploadMultiPartMediaAsync(IFormFile? avatar, string? key)
    {
        if (avatar == null || string.IsNullOrEmpty(key))
        {
            return null;
        }
        var request = new AwsRequest
        {
            ContentLength = avatar.Length,
            Key = key,
            InputStream = avatar!.OpenReadStream(),
        };

        var response = await awsAmazonService.UploadMultiplePartAsync(request);

        if (!response.IsSuccess)
        {
            logger.Information(
                "\nUpdate MultiPart has had error with file upload: {error}.\n",
                response.Error
            );
            return null;
        }

        logger.Information(
            "\nUpdate MultiPart success full with the path: {path}.\n",
            response.S3UploadedPath
        );
        return key;
    }
}
