using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Warehouses;
using Mediator;

namespace Application.Features.Branches.Branch.Commands.Create
{
    public class CreateBranchHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<CreateBranchCommand, CreateBranchResponse>
    {
        public async ValueTask<CreateBranchResponse> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
        {
            Domain.Aggregates.Branches.Branch mappingBranch = mapper.Map<Domain.Aggregates.Branches.Branch>(request);
            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);
                Domain.Aggregates.Branches.Branch branch = await unitOfWork.Repository<Domain.Aggregates.Branches.Branch>().AddAsync(mappingBranch, cancellationToken);

                await unitOfWork.SaveAsync(cancellationToken);

                //await unitOfWork.CommitAsync(cancellationToken);
                //auto add warehouse
                var warehouse = new Warehouse
                {
                    Name = $"Kho của {branch.Name}",
                    Code = $"WH-{branch.Code}",
                    Description = "Kho mặc định được tạo cùng chi nhánh.",
                    BranchId = branch.Id,
                    ReorderLevel = 0,
                    Status = 0,
                };
                await unitOfWork.Repository<Warehouse>().AddAsync(warehouse, cancellationToken);

                await unitOfWork.SaveAsync(cancellationToken);

                await unitOfWork.CommitAsync(cancellationToken);
                return new CreateBranchResponse
                {
                    Message = "Branch created successfully"
                };
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                return new CreateBranchResponse
                {
                    Message = ex.Message
                };
            }
        }
    }
}
