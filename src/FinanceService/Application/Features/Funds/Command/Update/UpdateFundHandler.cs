using Application.Common.Interfaces.UnitOfWorks;
using Ardalis.GuardClauses;
using AutoMapper;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using System.Data.Common;

namespace Application.Features.Funds.Command.Update
{
    public class UpdateFundHandler(IUnitOfWork unitOfWork, IMapper mapper) :
         IRequestHandler<UpdateFundCommand>
    {
        public async ValueTask<Unit> Handle(UpdateFundCommand command, CancellationToken cancellationToken)
        {
    
            Fund fund =
                await unitOfWork
                    .Repository<Fund>()
                    .FindByConditionAsync(
                        new GetFundByIdSpecification(long.Parse(command.FundId)),
                        cancellationToken
                    )
                ?? throw new  Application.Common.Exceptions.NotFoundException(
                    [Messager.Create<Fund>().Message(MessageType.Found).Negative().BuildMessage()]
                );

            mapper.Map(command.updateFundModel, fund);

            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);


                await unitOfWork.Repository<Fund>().UpdateAsync(fund);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);

                return Unit.Value;
            }

            catch
            {

                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
