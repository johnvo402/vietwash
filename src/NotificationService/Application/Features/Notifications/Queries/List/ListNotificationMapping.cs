using System.Linq.Expressions;
using Domain.Aggregates.Notifications;

namespace Application.Features.Notifications.Queries.List
{
    public class ListNotificationMapping
    {
        public static Expression<Func<Notification, ListNotificationResponse>> Selector()
        {
            return notification => new ListNotificationResponse
            {
                Id = notification.Id,
                CreatedAt = notification.CreatedAt,
                Title = notification.Title,
                Content = notification.Content,
                ContentHtml = notification.ContentHtml,
                Data = notification.Data,
                IsRead = notification.IsRead,
            };
        }
    }
}
