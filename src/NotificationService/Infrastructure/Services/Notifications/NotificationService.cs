using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections;
using Domain.Aggregates.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services.Notifications
{
    public class NotificationService(
        IUnitOfWork unitOfWork,
        IHubContext<NotificationHub> _hubContext
    ) : INotificationService
    {
        public async Task SendAsync(NotificationModel request, CancellationToken cancellationToken)
        {
            var template = await unitOfWork
                .Repository<NotificationTemplate>()
                .FindByIdAsync(request.TemplateId);
            if (template == null)
                throw new Exception("Template not found");
            string? content = null;
            string? contentHtml = null;
            if (request.Parameters != null)
            {
                content = ReplaceParameters(template.Content, request.Parameters);
                contentHtml = ReplaceParameters(template.ContentHtml, request.Parameters);
            }

            var notifications = new List<Notification>();
            if (!DateTimeOffset.TryParse(request.Time, out var createdAt))
            {
                createdAt = DateTimeOffset.UtcNow;
            }
            foreach (var userId in request.UserIds)
            {
                var notification = new Notification
                {
                    TemplateId = request.TemplateId,
                    UserId = userId,
                    Parameters = request.Parameters,
                    Title = template.Title,
                    Content = content,
                    ContentHtml = contentHtml,
                    Data = request.Data,
                    CreatedAt = createdAt,
                };

                var notificationDto = new NotificationProjection
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Content = content,
                    ContentHtml = contentHtml,
                    Data = notification.Data,
                    CreatedAt = notification.CreatedAt,
                };

                await _hubContext
                    .Clients.Group($"user:{userId}")
                    .SendAsync("ReceiveNotification", notificationDto, cancellationToken);

                notifications.Add(notification);
            }

            try
            {
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
                await unitOfWork
                    .Repository<Notification>()
                    .AddRangeAsync(notifications, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private string ReplaceParameters(string content, Dictionary<string, string> parameters)
        {
            var result = content;
            foreach (var param in parameters)
            {
                result = result.Replace($"{{{{{param.Key}}}}}", param.Value);
            }
            return result;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await unitOfWork
                .Repository<Notification>()
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task ReadAsync(long id)
        {
            var notification = await unitOfWork.Repository<Notification>().FindByIdAsync(id);
            if (notification is not null)
            {
                try
                {
                    _ = await unitOfWork.BeginTransactionAsync();
                    await unitOfWork.Repository<Notification>().UpdateAsync(notification);
                    await unitOfWork.SaveAsync();
                    await unitOfWork.CommitAsync();
                }
                catch (Exception)
                {
                    await unitOfWork.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task ReadAllAsync(string userId)
        {
            var notis = unitOfWork
                .Repository<Notification>()
                .QueryAsync(n => n.UserId == userId && !n.IsRead)
                .ToList();

            notis.ForEach(n => n.IsRead = true);
            try
            {
                _ = await unitOfWork.BeginTransactionAsync();
                await unitOfWork.Repository<Notification>().UpdateRangeAsync(notis);
                await unitOfWork.SaveAsync();
                await unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
