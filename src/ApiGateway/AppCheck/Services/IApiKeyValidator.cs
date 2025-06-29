namespace ApiGateway.AppCheck.Services
{
    public interface IApiKeyValidator
    {
        Task<bool> ValidateAsync(HttpContext context);
    }
}
