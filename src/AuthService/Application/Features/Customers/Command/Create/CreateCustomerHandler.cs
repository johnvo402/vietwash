using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Accounts;
using Mediator;
using Contracts.ApiWrapper;
using Contracts.Utils;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Command.Create
{
	public class CreateCustomerHandler(IUnitOfWork unitOfWork)
	: IRequestHandler<CreateCustomerCommand, Result>
	{
		public async ValueTask<Result> Handle(
			CreateCustomerCommand command,
			CancellationToken cancellationToken
		)
		{
			var branchIds = await unitOfWork
				.Repository<BranchAccount>()
				.QueryAsync()
				.Select(x => x.BranchId)
				.Distinct()
				.ToListAsync(cancellationToken);

			string code = Generator.GenerateAccountCode(ROLE.CUSTOMER);
			Account mappingAccount = command.ToAccount(code, branchIds);

			try
			{
				_ = await unitOfWork.BeginTransactionAsync(cancellationToken);

				Account user = await unitOfWork
					.Repository<Account>()
					.AddAsync(mappingAccount, cancellationToken);

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
