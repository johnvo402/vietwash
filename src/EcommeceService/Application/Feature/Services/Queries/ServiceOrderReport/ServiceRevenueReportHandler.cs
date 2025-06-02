using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.QueryStringProcessing;
using Application.Feature.Orders.Queries.List;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Domain.Aggregates.Services;
using Infrastructure.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;


namespace Application.Feature.Services.Queries.ServiceOrderReport
{
	public class ServiceRevenueReportHandler(
			IUnitOfWork unitOfWork
			) : IRequestHandler<ServiceRevenueReportQuery, List<ServiceRevenueReportResponse>>
	{
		public async ValueTask<List<ServiceRevenueReportResponse>> Handle(ServiceRevenueReportQuery request, CancellationToken cancellationToken)
		{
			try
			{
				var queryParamRequest = new QueryParamRequest();

				var orders = await unitOfWork.Repository<Order>().ListAsync(
				new GetOrdersForServiceRevenueReportSpecification(request.StartDate, request.EndDate),
				queryParamRequest,
				cancellationToken);

				// Lấy tất cả OrderItem từ các Order
				var orderItems = orders.SelectMany(o => o.OrderItems).ToList();

				// Nhóm OrderItem theo UnitId (từ UnitRelation) và ServiceId
				var serviceOrderItems = orderItems
					.GroupBy(oi => new { oi.UnitRelation.ReferenceId, oi.ServiceId })
					.Select(g => new
					{
						UnitId = g.Key.ReferenceId,
						ServiceId = g.Key.ServiceId,
						OrderItems = g.ToList(),
						Orders = orders.Where(o => g.Select(oi => oi.OrderId).Distinct().Contains(o.Id)).ToList()
					})
					.ToList();

				var reports = new List<ServiceRevenueReportResponse>();
				foreach (var serviceOrderItem in serviceOrderItems)
				{
					//var service =
					//	await unitOfWork
					//		.Repository<Service>()
					//		.FindByConditionAsync(
					//			new GetServiceWithIncludeByIdSpecification(serviceOrderItem.ServiceId),
					//			cancellationToken
					//		)
					//	?? throw new NotFoundException(
					//		[Messager.Create<Service>().Message(MessageType.Found).Negative().BuildMessage()]
					//	);
					var firstOrderItem = serviceOrderItem.OrderItems.First();
					var service = firstOrderItem.Service;
					var unitRelation = firstOrderItem.UnitRelation;
					var unit = unitRelation?.ReferenceId;

					// Tính các giá trị
					// Doanh thu trước giảm giá của từng item trong order
					var orderitemGrossRevenue = serviceOrderItem.OrderItems.Sum(oi => oi.Price * oi.Quantity);

					// Tính TotalDiscount
					var totalDiscount = 0m;
					foreach (var order in serviceOrderItem.Orders)
					{
						// Tính tổng doanh thu trước giảm giá của Order
						var orderGrossRevenue = order.OrderItems.Sum(oi => oi.Price * oi.Quantity);
						if (orderGrossRevenue == 0)
							continue;

						// Tính số tiền giảm giá của Order
						var orderDiscount = order.DiscountFixed
							? (orderGrossRevenue * order.DiscountValue) / 100 // Giảm giá theo %
							: order.DiscountValue; // Giảm giá theo số tiền cố định

						// Tính tỷ lệ doanh thu của các OrderItem trong cặp (Service, Unit) so với toàn bộ Order
						var groupGrossRevenue = serviceOrderItem.OrderItems
							.Where(oi => oi.OrderId == order.Id)
							.Sum(oi => oi.Price * oi.Quantity);
						var ratio = groupGrossRevenue / orderGrossRevenue;

						// Phân bổ giảm giá cho nhóm (Service, Unit)
						totalDiscount += orderDiscount * ratio;
					}
					
					// Doanh thu sau giảm giá
					var totalRevenue = orderitemGrossRevenue - totalDiscount;

					// Làm tròn  2 chữ số sau dấu chấm
					totalDiscount = Math.Round(totalDiscount, 2);
					totalRevenue = Math.Round(totalRevenue, 2);
					var relatedOrders = serviceOrderItem.Orders;
					var totalNetRevenue = relatedOrders
						.SelectMany(o => o.OrderPayments)
						.Sum(op => op.Amount);

					reports.Add(new ServiceRevenueReportResponse
					{
						ServiceId = service.Id,
						ServiceName = service.Name,
						//UnitId = unit.Id,
						//UnitName = unit.Name,
						TotalOrderCount = relatedOrders.Count,
						TotalNetRevenue = orderitemGrossRevenue,
						TotalDiscount = totalDiscount,
						TotalRevenue = totalRevenue
					});

					reports = reports.OrderByDescending(r => r.TotalOrderCount).ToList();
				}

				return reports;

			}
			catch (Exception ex)
			{
				throw;
			}
		}
	}
}
