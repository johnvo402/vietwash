using Wangkanai.Detection.Models;
using Wangkanai.Detection.Services;

namespace ApiGateway.AppCheck.Extensions;

public static class DeviceDetectionExtension
{
    public static bool IsWeb(this IDetectionService detection)
    {
        var device = detection.Device.Type;
        var browser = detection.Browser.Name.ToString().ToLowerInvariant();

        bool isBrowser =
            browser.Contains("chrome")
            || browser.Contains("safari")
            || browser.Contains("firefox")
            || browser.Contains("edge");

        return device == Device.Desktop || (device != Device.Desktop && isBrowser);
    }

    public static bool IsMobileOrTablet(this IDetectionService detection)
    {
        var device = detection.Device.Type;
        var userAgent = detection.UserAgent.ToString().ToLowerInvariant();
        bool isMobileDevice =
            device == Device.Mobile
            || device == Device.Tablet
            || userAgent.Contains("android")
            || userAgent.Contains("iphone")
            || userAgent.Contains("ipad")
            || userAgent.Contains("mobile")
            || userAgent.Contains("flutter");

        return isMobileDevice;
    }

    public static Device GetDeviceType(this IDetectionService detection)
    {
        return detection.Device.Type;
    }

    public static string GetPlatform(this IDetectionService detection)
    {
        return detection.Platform.Name.ToString();
    }

    public static string GetBrowser(this IDetectionService detection)
    {
        return detection.Browser.Name.ToString();
    }
}
