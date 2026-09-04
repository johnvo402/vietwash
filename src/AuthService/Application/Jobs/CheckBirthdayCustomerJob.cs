using System.Linq.Expressions;
using Application.Common.Interfaces.Registers;
using Application.Common.Interfaces.Services.Mail;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Application.Common.Interfaces.Services.Notifications;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Notification_Grpc;

namespace Application.Jobs
{
    public class UserDTO
    {
        public long? Id { get; set; }
        public string Email { get; set; } = default!;
        public string? DisplayName { get; set; }
    }

    public class CheckBirthdayCustomerJob : IJob
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMailService _mailer;
        private readonly INotificationGrpc _notification;

        public CheckBirthdayCustomerJob(
            IUnitOfWork unitOfWork,
            IMailService mailer,
            INotificationGrpc notification
        )
        {
            _unitOfWork = unitOfWork;
            _mailer = mailer;
            _notification = notification;
        }

        private Expression<Func<Account, UserDTO>> Selector()
        {
            return account => new UserDTO
            {
                Id = account.Id,
                DisplayName = account.DisplayName,
                Email = account.Email!,
            };
        }

        public async Task ExecuteAsync()
        {
            var now = DateOnly.FromDateTime(DateTime.UtcNow);
            var listCustomer = await _unitOfWork
                .DynamicReadOnlyRepository<Account>()
                .ListAsync(
                    new GetCustomerByBirthDaySpecification(now),
                    new QueryParamRequest(),
                    Selector(),
                    cancellationToken: default
                );
            foreach (var customer in listCustomer)
            {
                _ = await _mailer.SendWithTemplateAsync(
                    new MailTemplateData()
                    {
                        DisplayName = "VietWash",
                        Subject = "Chúc mừng sinh nhật!",
                        To = [customer.Email],
                        Template = new("HappyBirthday", customer.DisplayName ?? string.Empty),
                    }
                );
                try
                {
                    var notifySend = new SendNotificationRequest
                    {
                        TemplateId = "happy_birthday",
                        Time = DateTimeOffset.UtcNow.ToString(),
                    };
                    notifySend.Parameters["customer_name"] = customer.DisplayName;
                    notifySend.Parameters["voucher_expiry"] = now.AddDays(7)
                        .ToString("HH:mm:ss dd/MM/yyyy");

                    notifySend.UserIds.Add(customer.Id!.Value.ToString());
                    await _notification.SendNotifyAsync(notifySend);
                }
                catch (System.Exception)
                {
                    throw;
                }
            }
        }
    }
}
