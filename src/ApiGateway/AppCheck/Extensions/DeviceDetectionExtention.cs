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

    public static bool IsMobileOrTablet(this IDetectionService detection)
    {
        var device = detection.Device.Type;
        var browser = detection.Browser.Name.ToString().ToLower();
        var platform = detection.Platform.Name.ToString().ToLower();
        var userAgent = detection.UserAgent.ToString().ToLower();
        Console.WriteLine($"[DEBUG] UA: {userAgent}");
        Console.WriteLine($"[DEBUG] Device: {device}");
        Console.WriteLine($"[DEBUG] Browser: {browser}");
        Console.WriteLine($"[DEBUG] Platform: {platform}");
        // Xác định mobile/tablet từ Device.Type hoặc từ User-Agent thủ công
        bool isMobileDevice =
            device == Device.Mobile
            || device == Device.Tablet
            || userAgent.Contains("android")
            || userAgent.Contains("iphone")
            || userAgent.Contains("ipad")
            || userAgent.Contains("mobile"); // Flutter app

        // Xác định browser là unknown hoặc là Dart runtime
        bool isUnknownBrowser =
            string.IsNullOrEmpty(browser)
            || browser.Contains("unknown")
            || browser.Contains("others");

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
