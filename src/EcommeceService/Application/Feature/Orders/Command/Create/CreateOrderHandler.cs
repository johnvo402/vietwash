using System.Data.Common;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Queries.Detail;
using AutoMapper;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;

namespace Application.Feature.Orders.Command.Create
{
    public class CreateOrderHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
    {
        public async ValueTask<CreateOrderResponse> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken
        )
        {
            var order = mapper.Map<Order>(request);
            order.Code = $"ORD-{DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()[^6..]}";
            order.Amount = request.OrderItems.Sum(i => i.Price * i.Quantity);

            decimal discountValue = request.DiscountValue ?? 0m;
            order.Total =
                request.DiscountType == null ? order.Amount
                : request.DiscountType.Value ? order.Amount * (1 - discountValue / 100)
                : order.Amount - discountValue;
            order.OrderDate = DateTimeOffset.UtcNow;

            if (request.PaymentAmount < order.Total)
            {
                throw new BadRequestException(
                    [
                        Messager
                            .Create<Order>()
                            .Property(x => x.OrderPayments)
                            .Message(MessageType.Valid)
                            .Negative()
                            .Build(),
                    ]
                );
            }

            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(
                    cancellationToken
                );

                var orderRes = await unitOfWork
                    .Repository<Order>()
                    .AddAsync(order, cancellationToken);

                if (request.PaymentAmount > 0)
                {
                    var orderPayment = new OrderPayment
                    {
                        OrderId = order.Id,
                        PaymentMethod = request.PaymentMethod,
                        Amount = request.PaymentAmount,
                        PaymentDate = DateTimeOffset.UtcNow,
                    };
                    order.OrderPayments.Add(orderPayment);
                }

                await unitOfWork.SaveAsync(cancellationToken);

                await unitOfWork.CommitAsync(cancellationToken);
                var newOrder = await unitOfWork
                    .Repository<Order>()
                    .FindByConditionAsync<GetOrderDetailResponse>(
                        new GetOrderByIdSpecification(orderRes.Id),
                        cancellationToken
                    );
                var response = mapper.Map<CreateOrderResponse>(newOrder);
                return response;
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
