using Application.Common.Errors;
using Application.Common.Interfaces.Services.Mail;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Contracts.Dtos.Models;
using Contracts.Dtos.Requests;
using Contracts.Extensions;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Accounts.Commands.RequestResetPassword;

public class RequestResetAccountPasswordHandler(
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    IMailService mailer
) : IRequestHandler<RequestResetAccountPasswordCommand, Result>
{
    public async ValueTask<Result> Handle(
        RequestResetAccountPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        Account? user = await unitOfWork
            .DynamicReadOnlyRepository<Account>(false)
            .FindByConditionAsync(
                new GetUserByEmailForgotPasswordSpecification(command.Email),
                cancellationToken
            );
        if (user == null)
        {
            return Result.Failure(
                new NotFoundError(
                    "Account not found",
                    Messager.Create<Account>().Message(MessageType.Found).Negative().Build()
                )
            );
        }

        string token = StringExtension.GenerateRandomString(40);

        DateTimeOffset expiredTime = DateTimeOffset.UtcNow.AddHours(
            configuration.GetValue<int>("ForgotPasswordExpiredTimeInHour")
        );
        AccountResetPassword userResetPassword = new()
        {
            Token = token,
            AccountId = user.Id,
            Expiry = expiredTime,
        };

        await unitOfWork
            .Repository<AccountResetPassword>()
            .DeleteRangeAsync(user.AccountResetPasswords!);
        await unitOfWork
            .Repository<AccountResetPassword>()
            .AddAsync(userResetPassword, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);

        string domain = configuration.GetValue<string>("ForgotPassowordUrl")!;
        var link = new UriBuilder(domain) { Query = $"token={token}&id={user.PublicId}" };
        string expiry = expiredTime.ToLocalTime().ToString("dd/MM/yyyy hh:mm:ss");

        _ = await mailer.SendWithTemplateAsync(
            new MailTemplateData()
            {
                DisplayName = "The template Reset password",
                Subject = "Reset password",
                To = [user.Email],
                Template = new(
                    "ForgotPassword",
                    new ResetPasswordModel() { ResetLink = link.ToString(), Expiry = expiry }
                ),
            }
        );
        return Result.Success();
    }
}
