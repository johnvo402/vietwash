using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;

namespace Application.Common.Interfaces.UnitOfWorks;

public interface IRepositoryFunction<T>
    where T : new()
{
    Task<PaginationResponse<T>> ExecuteFunctionWithPagingAsync(
        string functionName,
        IDictionary<string, object?> parameters,
        string? sort,
        int page,
        int pageSize,
        string defaultSort,
        CancellationToken cancellationToken = default
    );
}
