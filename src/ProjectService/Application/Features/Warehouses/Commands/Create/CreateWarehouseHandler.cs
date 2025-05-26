using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Branches.Branch.Commands.Create;
using AutoMapper;
using Domain.Aggregates.Warehouses;
using Mediator;

namespace Application.Features.Warehouses.Commands.Create
{
    public class CreateWarehouseHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<CreateWarehouseCommand, CreateWarehouseResponse>
    {
        public async ValueTask<CreateWarehouseResponse> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
        {
            Warehouse mappingWarehouse = mapper.Map<Warehouse>(request);
            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);
                Warehouse warehouse = await unitOfWork.Repository<Warehouse>().AddAsync(mappingWarehouse, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);

                await unitOfWork.CommitAsync(cancellationToken);
                return new CreateWarehouseResponse
                {
                    Message = "Warehouse created successfully"
                };
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                return new CreateWarehouseResponse
                {
                    Message = ex.Message,
                };
            }
        }
    }
}
