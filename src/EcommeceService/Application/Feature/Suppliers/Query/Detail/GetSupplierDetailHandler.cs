using Application.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Services;
using Domain.Aggregates.Suppliers;
using Domain.Aggregates.Suppliers.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;


namespace Application.Feature.Suppliers.Query.Detail
{
	public class GetSupplierDetailHandler(IUnitOfWork unitOfWork, IMapper mapper)
	: IRequestHandler<GetSupplierDetailQuery, GetSupplierDetailResponse>
	{
		public async ValueTask<GetSupplierDetailResponse> Handle(
			GetSupplierDetailQuery query,
			CancellationToken cancellationToken
		)
		{
			var supplier =
				await unitOfWork
					.Repository<Supplier>()
					.FindByConditionAsync(
						new GetSupplierWithIncludeByIdSpecification(query.SupplierId),
						cancellationToken
					)
				?? throw new NotFoundException(
					[Messager.Create<Supplier>().Message(MessageType.Found).Negative().BuildMessage()]
				);

			var response = mapper.Map<GetSupplierDetailResponse>(supplier);
			return response;
		}
	}
}
