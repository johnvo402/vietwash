using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Mediator;
using Domain.Aggregates.Branches.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Application.Common.Exceptions;
using System.Data.Common;

namespace Application.Features.Branches.Branch.Commands.Update
{
    public class UpdateBranchHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<UpdateBranchCommand, UpdateBranchResponse>
    {
        public async ValueTask<UpdateBranchResponse> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
        {
            Domain.Aggregates.Branches.Branch branch = await unitOfWork.Repository<Domain.Aggregates.Branches.Branch>().FindByConditionAsync(
                                                                new GetBranchByIdWithoutIncludeSpecification(request.BranchId), cancellationToken
                                                                ) ?? throw new NotFoundException(
                                                                    [Messager.Create<Domain.Aggregates.Branches.Branch>().Message(MessageType.Found).Negative().BuildMessage()]
                                                                    );
            mapper.Map(request.Branch, branch);
            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);
                await unitOfWork.Repository<Domain.Aggregates.Branches.Branch>().UpdateAsync(branch);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
                return mapper.Map<UpdateBranchResponse>(branch);

            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
