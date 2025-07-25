using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Accounts.Commands.Update;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Customers.Command.Update
{
    public class UpdateCustomerHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateCustomerCommand, Result>
    {
        public async ValueTask<Result> Handle(
            UpdateCustomerCommand command,
            CancellationToken cancellationToken
        )
        {
            Account? customer = await unitOfWork
                .DynamicReadOnlyRepository<Account>()
                .FindByConditionAsync(
                    new GetAccountByIdSpecification(command.AccountId),
                    cancellationToken
                );
            if (customer == null)
            {
                return Result.Failure(
                    new NotFoundError(
                        "Account not found",
                        Messager
                            .Create<Account>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }

            customer.FromUpdateCustomer(command.Account!);

            // update default claim

            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                await unitOfWork.Repository<Account>().UpdateAsync(customer);
                await unitOfWork.SaveAsync(cancellationToken);

                await unitOfWork.CommitAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
