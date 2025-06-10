using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Command.Update;
using Application.Feature.Units.Command.Update;
using AutoMapper;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Domain.Aggregates.Services;
using Infrastructure.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Application.Feature.Orders.Command.UpdateStatus
{
    public class UpdateStatusHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper
    )
        : IRequestHandler<UpdateStatusCommand, UpdateStatusResponse>
    {
        public async ValueTask<UpdateStatusResponse> Handle(UpdateStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Tìm Order theo OrderId
                Order order =
                await unitOfWork
                    .Repository<Order>()
                    .FindByConditionAsync(
                        new GetOrderByIdSpecification(long.Parse(request.OrderId)),
                        cancellationToken
                    )
                ?? throw new NotFoundException(
                    [Messager.Create<Order>().Message(MessageType.Found).Negative().BuildMessage()]
                );
                //Order order =
                //	await unitOfWork
                //		.Repository<Order>()
                //		.FindByIdAsync(Ulid.Parse(request.OrderId))
                //	?? throw new NotFoundException(
                //		[Messager.Create<Order>().Message(MessageType.Found).Negative().BuildMessage()]
                //	);



                if (request.Status.HasValue)
                {
                    if (request.Status.Value < order.Status)
                        throw new BadRequestException(
                            [Messager.Create<Order>().Property(x => x.Status).Message(MessageType.Valid).Negative().Build()]);

                    order.UpdateStatus(request.Status.Value);
                }
                using var transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

                // Cập nhật và lưu thay đổi
                await unitOfWork.Repository<Order>().UpdateAsync(order);
                await unitOfWork.SaveAsync(cancellationToken);

                // Commit transaction
                await transaction.CommitAsync(cancellationToken);
                return new UpdateStatusResponse
                {
                    Message = "Order updated successfully"
                };
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }

        }
    }
}
