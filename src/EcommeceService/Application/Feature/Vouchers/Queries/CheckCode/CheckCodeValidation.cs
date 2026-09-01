using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Users;
using FluentValidation;

namespace Application.Feature.Vouchers.Queries.CheckCode
{
    public class CheckCodeValidation : AbstractValidator<CheckCodeQuery>
    {
        public CheckCodeValidation(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0)
                .MustAsync(
                    (customerId, cancellationToken) =>
                        unitOfWork
                            .Repository<User>()
                            .AnyAsync(
                                x =>
                                    x.Id == customerId
                                    && x.Status == ActivationStatus.Active,
                                cancellationToken
                            )
                )
                .WithState(x =>
                    Messager.Create<User>().Message(MessageType.Existence).Negative().Build()
                );

            RuleFor(x => x.VoucherCode).NotEmpty();
        }
    }
}
