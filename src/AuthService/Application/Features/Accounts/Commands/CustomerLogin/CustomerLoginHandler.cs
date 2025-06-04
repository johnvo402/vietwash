using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Specifications;
using Domain.Otp;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using Serilog;


namespace Application.Features.Accounts.Commands.CustomerLogin;

public class CustomerLoginHandler(IUnitOfWork unitOfWork, ISmsOtpClient _client)
    : IRequestHandler<CustomerLoginCommand, CustomerLoginResponse>
{
    public async ValueTask<CustomerLoginResponse> Handle(
        CustomerLoginCommand request,
        CancellationToken cancellationToken
    )
    {
        Account user =
            await unitOfWork
                .Repository<Account>()
                .FindByConditionAsync(
                    new GetAccountByPhoneNumberSpecification(request.PhoneNumber),
                    cancellationToken
                )
            ?? throw new NotFoundException(
                [Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()]
            );
        if (!(user.Status == AccountStatus.Active))
        {
            throw new BadRequestException(
                [
                    Messager
                        .Create<Account>()
                        .Property(x => x.Status)
                        .Message(MessageType.Active)
                        .Negative()
                        .BuildMessage(),
                ]
            );
        }
        await _client.CreatePinAsync(
                new CreatePinRequest
                {
                    To = request.PhoneNumber,
                    AccountId = user.Id,
                    Type = AccountActivityType.Login,
                },
                cancellationToken
            );

        return new CustomerLoginResponse { Message = "SendOTP" };
    }
}
