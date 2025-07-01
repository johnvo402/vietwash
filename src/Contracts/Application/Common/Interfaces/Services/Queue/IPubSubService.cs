namespace Application.Common.Interfaces.Services.DistributedCache;

public interface IPubSubService
{
    /// <summary>
    /// Gửi thông điệp đến tất cả các subscriber đang lắng nghe.
    /// </summary>
    Task<bool> PublishAsync<T>(T payload, string publicName);

    /// <summary>
    /// Đăng ký xử lý khi nhận được thông điệp theo kiểu dữ liệu cụ thể.
    /// </summary>
    void Subscribe<T>(Func<T, Task> handler, string publicName);

    /// <summary>
    /// Kiểm tra kết nối Redis.
    /// </summary>
    Task<bool> PingAsync();
}
