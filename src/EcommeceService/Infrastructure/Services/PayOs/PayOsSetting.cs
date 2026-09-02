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
    public static IReadOnlyList<string> GetErrors(PayOsSetting setting)
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
        return errors;
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
