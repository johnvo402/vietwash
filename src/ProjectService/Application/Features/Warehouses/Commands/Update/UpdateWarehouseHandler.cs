using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Warehouses;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using Domain.Aggregates.Warehouses.Specifications;

namespace Application.Features.Warehouses.Commands.Update
{
    public class UpdateWarehouseHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<UpdateWarehouseCommand, UpdateWarehouseResponse>
    {
        public async ValueTask<UpdateWarehouseResponse> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
        {
            Warehouse warehouse = await unitOfWork.Repository<Warehouse>()
                                                        .FindByConditionAsync(
                                                            new GetWarehouseByIdWithoutIncludeSpecification(request.WarehouseId), cancellationToken
                                                        ) ?? throw new NotFoundException(
                                                            [Messager.Create<Warehouse>().Message(MessageType.Found).Negative().BuildMessage()]
                                                            );
            mapper.Map(request.Warehouse, warehouse);
            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);
                await unitOfWork.Repository<Warehouse>().UpdateAsync(warehouse);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
                return mapper.Map<UpdateWarehouseResponse>( warehouse );
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
