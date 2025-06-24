using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Suppliers;
using Domain.Aggregates.Suppliers.Specifications;
using Mediator;

namespace Application.Feature.Suppliers.Query.Detail
{
    public class GetSupplierDetailHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetSupplierDetailQuery, Result<GetSupplierDetailResponse>>
    {
        public async ValueTask<Result<GetSupplierDetailResponse>> Handle(
            GetSupplierDetailQuery query,
            CancellationToken cancellationToken
        )
        {
            var supplier = await unitOfWork
                .DynamicReadOnlyRepository<Supplier>()
                .FindByConditionAsync(
                    new GetSupplierWithIncludeByIdSpecification(query.SupplierId),
                    cancellationToken
                );
            if (supplier == null)
            {
                return Result<GetSupplierDetailResponse>.Failure(
                    new NotFoundError(
                        "Supplier not found",
                        Messager
                            .Create<Supplier>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }

            var response = supplier.ToCreateUserResponse();
            return Result<GetSupplierDetailResponse>.Success(response);
        }
    }
}
