using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Application.Common.Interfaces.Services.Token;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using Serilog;
using SpeedSMS.PinService.Abstractions;
using SpeedSMS.PinService.Models;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Verify.V2.Service;
using Wangkanai.Detection.Services;

namespace Application.Features.Accounts.Commands.CustomerLogin;

public class CustomerLoginHandler(IUnitOfWork unitOfWork, ISpeedSmsPinClient _client)
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
        try
        {
            var accountSid = "AC1bea1c94d2fd1911a9d53cf264f8931d";
            var authToken = "e43b8d0529e4c6a60e0ac1706cadc113";
            TwilioClient.Init(accountSid, authToken);

            var verification = VerificationResource.Create(
                to: "+84383395692",
                channel: "sms",
                pathServiceSid: "VA21c2a85aeb96edec39c1f7114cd6f8f7"
            );
            Log.Logger.Information("Twilio OTP verification sent: {@Verification}", verification);
        }
        catch (Exception ex)
        {
            throw new Exception("Transaction rollback failed.", ex);
        }

        return new CustomerLoginResponse { Message = "SendOTP" };
    }
}
