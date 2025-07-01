using ApiGateway.AppCheck.Extensions;
using ApiGateway.AppCheck.Models;
using Microsoft.Extensions.Options;
using Wangkanai.Detection.Services;

namespace ApiGateway.AppCheck.Services
{
    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly ApiSettings _apiSettings;
        private readonly IDetectionService _detectionService;

        public ApiKeyValidator(
            IOptions<ApiSettings> apiSettings,
            IDetectionService detectionService
        )
        {
            _apiSettings = apiSettings.Value;
            _detectionService = detectionService;
        }

        public Task<bool> ValidateAsync(HttpContext context)
        {
            var request = context.Request;

            var apiKey = request.Headers["x-api-key"].FirstOrDefault();
            var platform = request.Headers["platform"].FirstOrDefault();
            var origin = request.Headers["origin"].FirstOrDefault();

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(platform))
                return Task.FromResult(false);

            if (_detectionService.IsWeb())
            {
                if (apiKey != _apiSettings.Web?.ApiKey || platform != _apiSettings.Web?.Platform)
                    return Task.FromResult(false);

                if (
                    _apiSettings.Web.Origin != null
                    && !_apiSettings.Web.Origin.Contains(origin ?? string.Empty)
                )
                    return Task.FromResult(false);
            }
            else if (_detectionService.IsMobileOrTablet())
            {
                if (
                    apiKey != _apiSettings.Mobile?.ApiKey
                    || platform != _apiSettings.Mobile?.Platform
                )
                    return Task.FromResult(false);
            }
            else
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
