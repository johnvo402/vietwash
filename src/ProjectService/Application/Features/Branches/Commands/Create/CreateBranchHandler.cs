using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Utils;
using Domain.Aggregates.Warehouses;
using Domain.Aggregates.Warehouses.Enums;
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
