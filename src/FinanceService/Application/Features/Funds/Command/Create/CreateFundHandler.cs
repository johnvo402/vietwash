using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Funds;
using Mediator;
using System.Data.Common;

namespace Application.Features.Funds.Command.Create
{
    public class CreateFundHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<CreateFundCommand>
    {
        public async ValueTask<Unit> Handle(CreateFundCommand request, CancellationToken cancellationToken)
        {
            var fund = mapper.Map<Fund>(request);

            if (fund == null)
            {
                return Unit.Value;
            }

            fund.Code = $"FU-{DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()[^6..]}";
            fund.TransactionDate = DateTimeOffset.UtcNow;

            try
            {
                 
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

            
                await unitOfWork.Repository<Fund>().AddAsync(fund, cancellationToken);

               
                await unitOfWork.SaveAsync(cancellationToken);

               
                await unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
          
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }

            return Unit.Value;
        }
    }
}
