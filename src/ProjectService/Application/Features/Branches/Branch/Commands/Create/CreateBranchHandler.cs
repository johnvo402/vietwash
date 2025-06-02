using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Contracts.Utils;
using Domain.Aggregates.Warehouses;
using Domain.Aggregates.Warehouses.Enums;
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

                if (string.IsNullOrEmpty(branch.Code))
                {
                    branch.Code = Generator.GenerateCode("BR", 6);
                }
                //auto add warehouse
                var warehouse = new Warehouse
                {
                    Name = $"{branch.Name} kho",
                    Code = $"WH-{branch.Code}",
                    Description = $"Kho mặc định được tạo cùng chi nhánh {branch.Name}.",
                    BranchId = branch.Id,
                    ReorderLevel = 0,
                    Status = WarehouseStatus.Active,
                };
                branch.Warehouses.Add(warehouse);
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
