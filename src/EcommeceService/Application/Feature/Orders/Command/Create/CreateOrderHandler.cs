using Application.Common.Exceptions;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Orders;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using System.Data.Common;


namespace Application.Feature.Orders.Command.Create
{
	public class CreateOrderHandler(
		IUnitOfWork unitOfWork,
		IMapper mapper
	) : IRequestHandler<CreateOrderCommand, Unit>
	{
		public async ValueTask<Unit> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
		{
			var order = mapper.Map<Order>(request);
			order.Code = $"ORD-{DateTimeOffset.UtcNow.Ticks.ToString()[^6..]}";
			order.Amount = request.OrderItems.Sum(i => i.Price);

			decimal discountValue = request.DiscountValue ?? 0m;
			order.Total = request.DiscountType == null
					? order.Amount // Không giảm giá nếu DiscountType null
					: request.DiscountType.Value
						? order.Amount * (1 - discountValue / 100) 
						: order.Amount - discountValue; 
			order.OrderDate = DateTimeOffset.UtcNow;

			// Kiểm tra nếu PaymentAmount không đủ để thanh toán
			if (request.PaymentAmount < order.Total)
			{
				throw new BadRequestException(
						[Messager
						.Create<Order>()
						.Property(x => x.OrderPayments)
						.Message(MessageType.Valid)
						.Negative()
						.Build()]
					);
			}

			try
			{
				DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

				await unitOfWork.Repository<Order>().AddAsync(order, cancellationToken);

				// Nếu có PaymentAmount, thêm OrderPayment
				if (request.PaymentAmount > 0)
				{
					var orderPayment = new OrderPayment
					{
						OrderId = order.Id, // Id sẽ được sinh sau khi AddAsync
						PaymentMethod = request.PaymentMethod,
						Amount = request.PaymentAmount,
						PaymentDate = DateTimeOffset.UtcNow
					};
					order.OrderPayments.Add(orderPayment);
				}

				await unitOfWork.SaveAsync(cancellationToken);

				await unitOfWork.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await unitOfWork.RollbackAsync(cancellationToken);
				throw;
			}
			return Mediator.Unit.Value;
		}
	}
}
