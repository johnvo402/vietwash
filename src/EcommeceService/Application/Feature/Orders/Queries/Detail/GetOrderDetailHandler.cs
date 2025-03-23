using Application.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Queries.Detail
{
	public class GetOrderDetailHandler(IUnitOfWork unitOfWork)
	: IRequestHandler<GetOrderDetailQuery, GetOrderDetailResponse>
	{
	public async ValueTask<GetOrderDetailResponse> Handle(
			GetOrderDetailQuery request, 
			CancellationToken cancellationToken
		) =>
		await unitOfWork
			.Repository<Order>()
			.FindByConditionAsync<GetOrderDetailResponse>(
				new GetOrderByIdSpecification(Ulid.Parse(request.OrderId)),
				cancellationToken
			)
		?? throw new NotFoundException(
			[Messager.Create<Order>().Message(MessageType.Found).Negative().BuildMessage()]
		);

	}
}
