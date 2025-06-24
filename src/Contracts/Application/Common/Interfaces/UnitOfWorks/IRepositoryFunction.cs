using System.Linq.Expressions;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;

namespace Application.Common.Interfaces.UnitOfWorks;

public interface IRepositoryFunction<T>
    where T : class
{
    Task<PaginationResponse<T>> PagedListAsync(
        string functionName,
        object[] parameters,
        string defaultSort,
        QueryParamRequest queryParam,
        CancellationToken cancellationToken = default
    );
    Task<IList<T>> ListAsync(
        string functionName,
        object[] parameters,
        string defaultSort,
        QueryParamRequest queryParam,
        CancellationToken cancellationToken = default
    );

    Task<IList<TResult>> ListAsync<TResult>(
        string functionName,
        object[] parameters,
        string defaultSort,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> mappingResult,
        CancellationToken cancellationToken = default
    )
        where TResult : class;

    Task<PaginationResponse<TResult>> PagedListAsync<TResult>(
        string functionName,
        object[] parameters,
        string defaultSort,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> mappingResult,
        CancellationToken cancellationToken = default
    );

    Task<PaginationResponse<TResult>> CursorPagedListAsync<TResult>(
        string functionName,
        object[] parameters,
        string defaultSort,
        QueryParamRequest queryParam,
        Expression<Func<T, TResult>> mappingResult,
        string? uniqueSort = null,
        CancellationToken cancellationToken = default
    )
        where TResult : class;
}
