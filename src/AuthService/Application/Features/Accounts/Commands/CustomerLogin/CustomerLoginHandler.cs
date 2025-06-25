using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Specifications;
using Domain.Otp;
using Infrastructure.Constants;
using Mediator;

namespace Application.Features.Accounts.Commands.CustomerLogin;

public class CustomerLoginHandler(
    IUnitOfWork unitOfWork,
    ISmsOtpClient _client,
    ICurrentAccount _currentAccount
) : IRequestHandler<CustomerLoginCommand, Result>
{
    public async ValueTask<Result> Handle(
        CustomerLoginCommand request,
        CancellationToken cancellationToken
    )
    {
        Account? user = await unitOfWork
            .DynamicReadOnlyRepository<Account>()
            .FindByConditionAsync(
                new GetAccountByPhoneNumberSpecification(request.PhoneNumber, ROLE.CUSTOMER),
                cancellationToken
            );
        long? accountId = null;
        if (user != null)
        {
            accountId = user.Id;
            if (!(user.Status == AccountStatus.Active))
            {
                return Result.Failure(
                    new BadRequestError(
                        "Account inactive",
                        Messager
                            .Create<Account>()
                            .Property(x => x.Status)
                            .Message(MessageType.Active)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }
        }

        var error = await _client.CreateAsync(
            new CreatePinRequest { To = request.PhoneNumber, ClientIp = _currentAccount.ClientIp! },
            cancellationToken
        );
        if (error != null)
        {
            return Result.Failure(error);
        }

        return Result.Success();
    }
}
