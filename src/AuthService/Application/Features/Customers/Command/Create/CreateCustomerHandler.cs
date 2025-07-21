using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Accounts;
using Mediator;
using Contracts.ApiWrapper;
using Contracts.Utils;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Application.Features.Common.Projections.Accounts;

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
			var branches = await unitOfWork
				.Repository<BranchAccount>()
				.QueryAsync()
				.GroupBy(x => x.BranchId)
				.Select(g => new BranchAccountModel
				{
					BranchId = g.Key,
					BranchName = g
						.Where(x => x.BranchName != null)
						.Select(x => x.BranchName)
						.FirstOrDefault()
				})
				.ToListAsync(cancellationToken);

			string code = Generator.GenerateAccountCode(ROLE.CUSTOMER);
			Account mappingAccount = command.ToAccount(code, branches);

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
