using Application.Common.Exceptions;
using Application.Common.Interfaces.Services.Mail;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Models;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Extensions;
using Mediator;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Accounts.Commands.RequestResetPassword;

public class RequestResetAccountPasswordHandler(
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    IMailer mailer
) : IRequestHandler<RequestResetAccountPasswordCommand>
{
    public async ValueTask<Unit> Handle(
        RequestResetAccountPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        Account user =
            await unitOfWork
                .CachedRepository<Account>()
                .FindByConditionAsync(
                    new GetUserByEmailForgotPasswordSpecification(command.Email),
                    cancellationToken
                )
            ?? throw new NotFoundException(
                [Messager.Create<Account>().Message(MessageType.Found).Negative().Build()]
            );

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
        var link = new UriBuilder(domain) { Query = $"token={token}&id={user.Id}" };
        string expiry = expiredTime.ToLocalTime().ToString("dd/MM/yyyy hh:mm:ss");

        _ = await mailer
            .Email()
            .SendWithTemplateAsync(
                new TemplateMailMetaData()
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
        return Unit.Value;
    }
}
