using Application.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Command.Delete
{
	public class DeleteOrderHandler(IUnitOfWork unitOfWork)
		: IRequestHandler<DeleteOrderCommand, Unit>
	{
		public async ValueTask<Unit> Handle(
			DeleteOrderCommand command,
			CancellationToken cancellationToken
		)
		{
			// Tìm Order theo OrderId
			var order = await unitOfWork.Repository<Order>()
				.FindByConditionAsync(
					new GetOrderByIdSpecification(command.OrderId),
					cancellationToken
				)
				?? throw new NotFoundException(
					[Messager.Create<Order>().Message(MessageType.Found).Negative().BuildMessage()]
				);

			// Cập nhật trạng thái thành Deleted thay vì xóa cứng
			order.Status = OrderStatus.Disabled;

			// Cập nhật Order trong cơ sở dữ liệu
			await unitOfWork.Repository<Order>().UpdateAsync(order);
			await unitOfWork.SaveAsync(cancellationToken);

			return Unit.Value;
		}
	}
}
