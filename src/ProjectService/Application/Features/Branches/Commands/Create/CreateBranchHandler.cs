using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Utils;
using Mediator;

namespace Application.Features.Branches.Commands.Create
{
    public class CreateBranchHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<CreateBranchCommand, Result>
    {
        public async ValueTask<Result> Handle(
            CreateBranchCommand request,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                request.Code = Generator.GenerateCode("BR", 6);
            }
            Domain.Aggregates.Branches.Branch mappingBranch = request.ToEntity();

            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );
                Domain.Aggregates.Branches.Branch branch = await unitOfWork
                    .Repository<Domain.Aggregates.Branches.Branch>()
                    .AddAsync(mappingBranch, cancellationToken);

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
