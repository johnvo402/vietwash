using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;

namespace Application.Common.Interfaces.UnitOfWorks;

public interface IRepositoryFunction<T>
    where T : class
{
    Task<PaginationResponse<T>> PagedListFunctionAsync(
        string functionName,
        object[] parameters,
        string defaultSort,
        QueryParamRequest queryParam,
        CancellationToken cancellationToken = default
    );
}
