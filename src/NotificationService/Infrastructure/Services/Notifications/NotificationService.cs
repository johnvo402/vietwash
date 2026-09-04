using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections;
using Domain.Aggregates.Notifications;
using Microsoft.AspNetCore.SignalR;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Serilog;

namespace Infrastructure.Services.Notifications
{
    public class NotificationService(
        IUnitOfWork unitOfWork,
        IHubContext<NotificationHub> _hubContext,
        TheDbContext db,
        ILogger logger
    ) : INotificationService
    {
        public async Task SendAsync(NotificationModel request, CancellationToken cancellationToken)
        {
            if (request.UserIds == null || request.UserIds.Count == 0 || request.UserIds.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Notification recipients are required");
            if (request.MessageId?.Length > 128)
                throw new ArgumentException("Notification message ID is too long");
            var recipients = request.UserIds.Distinct().Order(StringComparer.Ordinal).ToArray();
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(request.MessageId))
            {
                var recipientKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(new { request.TemplateId, Recipients = recipients }))));
                // PostgreSQL waits on a concurrent duplicate's transaction. Its receipt
                // and notification rows either commit together or both roll back.
                var inserted = await db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO notification_receipts (id, recipient_key, accepted_at)
                    VALUES ({request.MessageId}, {recipientKey}, {DateTimeOffset.UtcNow})
                    ON CONFLICT (id) DO NOTHING
                    """, cancellationToken);
                if (inserted == 0)
                {
                    var receipt = await db.Set<NotificationReceipt>().AsNoTracking()
                        .SingleAsync(x => x.Id == request.MessageId, cancellationToken);
                    if (receipt.RecipientKey != recipientKey)
                        throw new InvalidOperationException("Message ID cannot be reused for different recipients or templates");
                    await transaction.CommitAsync(cancellationToken);
                    return; // First accepted payload wins; retries never create or re-send duplicates.
                }
            }
            var template = await db.Set<NotificationTemplate>()
                .SingleOrDefaultAsync(x => x.Id == request.TemplateId, cancellationToken);
            if (template == null)
                throw new Exception("Template not found");
            string? content = template.Content;
            string? contentHtml = template.ContentHtml;
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
            foreach (var userId in recipients)
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

                notifications.Add(notification);
            }

            db.Set<Notification>().AddRange(notifications);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            // Acknowledgement means durable inbox persistence, not delivery to a
            // connected browser. Offline/reconnecting clients fetch the unread inbox.
            foreach (var notification in notifications)
            {
                try
                {
                    await _hubContext.Clients.Group($"user:{notification.UserId}").SendAsync("ReceiveNotification",
                        new NotificationProjection
                        {
                            Id = notification.Id, Title = notification.Title, Content = notification.Content,
                            ContentHtml = notification.ContentHtml, Data = notification.Data, CreatedAt = notification.CreatedAt,
                        }, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.Warning("Notification saved; realtime hint unavailable. NotificationId: {NotificationId}, Failure: {Failure}",
                        notification.Id, ex.GetType().Name);
                }
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
                    notification.IsRead = true;
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
