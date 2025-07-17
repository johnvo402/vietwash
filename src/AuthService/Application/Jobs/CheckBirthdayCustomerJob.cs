using System.Linq.Expressions;
using Application.Common.Interfaces.Registers;
using Application.Common.Interfaces.Services.Mail;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;

namespace Application.Jobs
{
    public class UserBirthday
    {
        public required string Email { get; set; }
        public required string DisplayName { get; set; }
    }

    public class CheckBirthdayCustomerJob : IJob
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMailService _mailer;

        public CheckBirthdayCustomerJob(IUnitOfWork unitOfWork, IMailService mailer)
        {
            _unitOfWork = unitOfWork;
            _mailer = mailer;
        }

        private Expression<Func<Account, UserBirthday>> Selector()
        {
            return account => new UserBirthday
            {
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
                        Template = new("HappyBirthday", customer.DisplayName),
                    }
                );
            }
        }
    }
}
