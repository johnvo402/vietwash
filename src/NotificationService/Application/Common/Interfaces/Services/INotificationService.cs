using Application.Features.Common.Projections;

namespace Application.Common.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendAsync(NotificationModel request, CancellationToken cancellationToken);
        Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken);
        Task ReadAsync(long id, CancellationToken cancellationToken);
        Task ReadAllAsync(string userId, CancellationToken cancellationToken);
    }
}
