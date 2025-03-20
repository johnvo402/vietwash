using Application.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Infrastructure.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Command.Update
{
	public class UpdateOrderHandler(
		IUnitOfWork unitOfWork,
		IMapper mapper
	)
		: IRequestHandler<UpdateOrderCommand, UpdateOrderResponse>
	{
	
		public async ValueTask<UpdateOrderResponse> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
		{
			// Tìm Order theo OrderId
			Order order =
				await unitOfWork
					.Repository<Order>()
					.FindByConditionAsync(
						new GetOrderByIdSpecification(Ulid.Parse(command.OrderId)),
						cancellationToken
					)
				?? throw new NotFoundException(
					[Messager.Create<Order>().Message(MessageType.Found).Negative().BuildMessage()]
				);

			// Ánh xạ thông tin từ command.Order sang order
			mapper.Map(command.Order, order);

			if (command.Status.HasValue)
			{
				order.Status = command.Status.Value; 
			}

			// Cập nhật OrderPayment nếu có PaymentAmount
			if (command.Order.PaymentAmount > 0)
			{
				var orderPayment = new OrderPayment
				{
					OrderId = order.Id,
					PaymentMethod = command.Order.PaymentMethod,
					Amount = command.Order.PaymentAmount,
					PaymentDate = DateTimeOffset.UtcNow
				};
				order.OrderPayments.Add(orderPayment);
			}

			// Bắt đầu giao dịch
			try
			{
				DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

				// Cập nhật Order
				await unitOfWork.Repository<Order>().UpdateAsync(order);
				await unitOfWork.SaveAsync(cancellationToken);

				// Commit giao dịch
				await unitOfWork.CommitAsync(cancellationToken);

				// Trả về response
				return mapper.Map<UpdateOrderResponse>(order);
			}
			catch (Exception)
			{
				// Rollback nếu có lỗi
				await unitOfWork.RollbackAsync(cancellationToken);
				throw;
			}
		}
	}
}
