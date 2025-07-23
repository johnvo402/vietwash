using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections.Accounts;
using Contracts.ApiWrapper;
using Contracts.Utils;
using Domain.Aggregates.Accounts;
using Infrastructure.Constants;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Command.Create
{
    public class CreateCustomerHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<CreateCustomerCommand, Result<CreateCustomerResponse>>
    {
        public async ValueTask<Result<CreateCustomerResponse>> Handle(
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
                    BranchName = g.Select(x => x.BranchName).FirstOrDefault() ?? string.Empty,
                })
                .ToListAsync();

            string code = Generator.GenerateAccountCode(ROLE.CUSTOMER);
            Account mappingAccount = command.ToAccount(code, branches);
            mappingAccount.CreateAccount();
            try
            {
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);

                Account user = await unitOfWork
                    .Repository<Account>()
                    .AddAsync(mappingAccount, cancellationToken);

                await unitOfWork.SaveAsync(cancellationToken);

                await unitOfWork.CommitAsync(cancellationToken);
                var response = user.ToCreateCustomerResponse();
                return Result<CreateCustomerResponse>.Success(response);
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
