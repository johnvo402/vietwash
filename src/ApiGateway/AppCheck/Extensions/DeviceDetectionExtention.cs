using Wangkanai.Detection.Models;
using Wangkanai.Detection.Services;

namespace ApiGateway.AppCheck.Extensions;

public static class DeviceDetectionExtension
{
    public static bool IsWeb(this IDetectionService _detection)
    {
        // Nếu device là Desktop hoặc nếu là Mobile/Tablet nhưng browser là Chrome, Safari (web browser)
        var device = _detection.Device.Type;
        var browser = _detection.Browser.Name.ToString().ToLower();
        var platform = _detection.Platform.Name.ToString().ToLower();

        bool isBrowser =
            browser.Contains("chrome")
            || browser.Contains("safari")
            || browser.Contains("firefox")
            || browser.Contains("edge");

        return device == Device.Desktop || (device != Device.Desktop && isBrowser);
    }

    public static bool IsMobileOrTablet(this IDetectionService _detection)
    {
        // Nếu device là Mobile/Tablet và browser không phải là web browser => khả năng cao là native app
        var device = _detection.Device.Type;
        var browser = _detection.Browser.Name.ToString().ToLower();
        var platform = _detection.Platform.Name.ToString().ToLower();

        bool isMobileDevice = device == Device.Mobile || device == Device.Tablet;

        // Nếu browser không rõ ràng hoặc là unknown => khả năng cao là native app
        bool isUnknownBrowser = string.IsNullOrEmpty(browser) || browser.Contains("unknown");

        return isMobileDevice && isUnknownBrowser;
    }

    public static Device GetDeviceType(this IDetectionService _detection)
    {
        return _detection.Device.Type;
    }

    public static string GetPlatform(this IDetectionService _detection)
    {
        return _detection.Platform.Name.ToString();
    }

    public static string GetBrowser(this IDetectionService _detection)
    {
        return _detection.Browser.Name.ToString();
    }
}
