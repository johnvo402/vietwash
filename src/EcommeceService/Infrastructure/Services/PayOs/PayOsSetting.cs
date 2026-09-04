using Application.Feature.Orders.Queries.GetLinkPayment;

namespace Infrastructure.Services.PayOs;

public sealed class PayOsSetting : IOrderPaymentSettings
{
    public string? ClientId { get; set; }

    public string? ApiKey { get; set; }

    public string? ChecksumKey { get; set; }

    public string? ReturnUrl { get; set; }

    public string? CancelUrl { get; set; }

    public string? WebhookUrl { get; set; }

    public bool IsEnabled { get; set; }
}

public static class PayOsSettingValidator
{
    public static IReadOnlyList<string> GetErrors(PayOsSetting setting, bool requirePublicHttps = false)
    {
        if (!setting.IsEnabled)
            return [];

        List<string> errors = [];
        Require(setting.ClientId, nameof(PayOsSetting.ClientId), errors);
        Require(setting.ApiKey, nameof(PayOsSetting.ApiKey), errors);
        Require(setting.ChecksumKey, nameof(PayOsSetting.ChecksumKey), errors);
        RequireAbsoluteHttpUrl(setting.ReturnUrl, nameof(PayOsSetting.ReturnUrl), errors);
        RequireAbsoluteHttpUrl(setting.CancelUrl, nameof(PayOsSetting.CancelUrl), errors);
        RequireAbsoluteHttpUrl(setting.WebhookUrl, nameof(PayOsSetting.WebhookUrl), errors);
        if (requirePublicHttps)
        {
            RequirePublicHttps(setting.ReturnUrl, nameof(PayOsSetting.ReturnUrl), errors);
            RequirePublicHttps(setting.CancelUrl, nameof(PayOsSetting.CancelUrl), errors);
            RequirePublicHttps(setting.WebhookUrl, nameof(PayOsSetting.WebhookUrl), errors);
            if (Uri.TryCreate(setting.WebhookUrl, UriKind.Absolute, out var webhook)
                && (webhook.AbsolutePath != "/Webhook/api/CompletedOrder" || webhook.Query.Length != 0))
                errors.Add("PayOsSetting:WebhookUrl must target /Webhook/api/CompletedOrder without a query string.");
        }
        return errors;
    }

    private static void RequirePublicHttps(string? value, string name, ICollection<string> errors)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps
            || uri.IsLoopback || uri.HostNameType != UriHostNameType.Dns
            || !uri.Host.Contains('.') || uri.Host.EndsWith(".local", StringComparison.OrdinalIgnoreCase)
            || uri.UserInfo.Length != 0 || uri.Fragment.Length != 0)
            errors.Add($"PayOsSetting:{name} must be a public HTTPS URL outside Development (no credentials or fragment).");
    }

    private static void Require(string? value, string name, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
            errors.Add($"PayOsSetting:{name} is required when PayOS is enabled.");
    }

    private static void RequireAbsoluteHttpUrl(
        string? value,
        string name,
        ICollection<string> errors
    )
    {
        if (
            string.IsNullOrWhiteSpace(value)
            || !Uri.TryCreate(value, UriKind.Absolute, out Uri? uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        )
            errors.Add(
                $"PayOsSetting:{name} must be an absolute HTTP or HTTPS URL when PayOS is enabled."
            );
    }
}
