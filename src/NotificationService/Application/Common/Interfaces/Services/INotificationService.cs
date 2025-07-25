using Application.Features.Common.Projections;

namespace Application.Common.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendAsync(NotificationModel request, CancellationToken cancellationToken);
        Task<int> GetUnreadCountAsync(string userId);
        Task ReadAsync(long id);
        Task ReadAllAsync(string userId);
    }
}
