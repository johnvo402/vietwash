using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Warehouses;
using Domain.Aggregates.Warehouses.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;

namespace Application.Features.Warehouses.Commands.Delete
{
    public class DeleteWarehouseHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteWarehouseCommand>
    {
        public async ValueTask<Unit> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
        {
            Warehouse warehouse = await unitOfWork.Repository<Warehouse>()
                                                    .FindByConditionAsync(
                                                        new GetWarehouseByIdWithoutIncludeSpecification(request.id), cancellationToken
                                                    ) ?? throw new NotFoundException(
                                                        [Messager.Create<Warehouse>().Message(MessageType.Found).Negative().BuildMessage()]
                                                        );
            await unitOfWork.Repository<Warehouse>().DeleteAsync( warehouse );
            await unitOfWork.SaveAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
