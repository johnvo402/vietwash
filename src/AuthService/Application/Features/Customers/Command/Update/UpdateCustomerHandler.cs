
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Accounts.Commands.Update;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts.Specifications;
using Domain.Aggregates.Accounts;
using Mediator;
using System.Data.Common;
using Contracts.ApiWrapper;

namespace Application.Features.Customers.Command.Update
{
	public class UpdateCustomerHandler(
	IUnitOfWork unitOfWork,
	IMediaUpdateService mediaUpdateService
) : IRequestHandler<UpdateCustomerCommand, Result>
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
						Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()
					)
				);
			}

			string? oldAvatar = customer.AvtUrl;

			customer.FromUpdateCustomer(command.Account!);

			customer.AvtUrl = command.Account!.AvtUrl;
			// update default claim

			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

				await unitOfWork.Repository<Account>().UpdateAsync(customer);
				await unitOfWork.SaveAsync(cancellationToken);

				await unitOfWork.CommitAsync(cancellationToken);

				await mediaUpdateService.DeleteAvatarAsync(oldAvatar);
				return Result.Success();
			}
			catch (Exception)
			{
				await mediaUpdateService.DeleteAvatarAsync(customer.AvtUrl);
				await unitOfWork.RollbackAsync(cancellationToken);
				throw;
			}
		}
	}
}
