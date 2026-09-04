using System.Linq.Expressions;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Contracts.Extensions.QueryExtensions;
using Infrastructure.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Contracts.Infrastructure.UnitOfWorks.Repositories
{
    public partial class RepositoryFunction<T>(IUnitOfWork unitOfWork) : IRepositoryFunction<T>
        where T : class
    {
        public async Task<PaginationResponse<TResult>> CursorPagedListAsync<TResult>(
            string functionName,
            object[] parameters,
            string defaultSort,
            QueryParamRequest queryParam,
            Expression<Func<T, TResult>> mappingResult,
            string? uniqueSort = null,
            CancellationToken cancellationToken = default
        )
            where TResult : class
        {
            return await unitOfWork
                .CallPostgreSqlFunction<T>(functionName, parameters)
                .Select(mappingResult)
                .Filter(queryParam.Filter)
                .Search(queryParam.Keyword, queryParam.Targets)
                .ToCursorPagedListAsync(
                    new CursorPaginationRequest(
                        queryParam.Before,
                        queryParam.After,
                        queryParam.PageSize,
                        queryParam.Sort.GetDefaultSort(),
                        uniqueSort ?? defaultSort
                    )
                );
        }

        public async Task<IList<T>> ListAsync(
            string functionName,
            object[] parameters,
            string defaultSort,
            QueryParamRequest queryParam,
            CancellationToken cancellationToken = default
        )
        {
            string uniqueSort = !string.IsNullOrEmpty(queryParam.Sort)
                ? queryParam.Sort
                : defaultSort;
            return await unitOfWork
                .CallPostgreSqlFunction<T>(functionName, parameters)
                .Filter(queryParam.Filter)
                .Search(queryParam?.Keyword, queryParam?.Targets)
                .Sort(uniqueSort)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<TResult>> ListAsync<TResult>(
            string functionName,
            object[] parameters,
            string defaultSort,
            QueryParamRequest queryParam,
            Expression<Func<T, TResult>> mappingResult,
            CancellationToken cancellationToken = default
        )
            where TResult : class
        {
            string uniqueSort = !string.IsNullOrEmpty(queryParam.Sort)
                ? queryParam.Sort
                : defaultSort;
            return await unitOfWork
                .CallPostgreSqlFunction<T>(functionName, parameters)
                .Select(mappingResult)
                .Filter(queryParam.Filter)
                .Search(queryParam?.Keyword, queryParam?.Targets)
                .Sort(uniqueSort)
                .ToListAsync(cancellationToken);
        }

        public async Task<PaginationResponse<T>> PagedListAsync(
            string functionName,
            object[] functionParameters,
            string defaultSort,
            QueryParamRequest queryParam,
            CancellationToken cancellationToken = default
        )
        {
            string uniqueSort = !string.IsNullOrEmpty(queryParam.Sort)
                ? queryParam.Sort
                : defaultSort;
            return await unitOfWork
                .CallPostgreSqlFunction<T>(functionName, functionParameters)
                .Filter(queryParam.Filter)
                .Search(queryParam?.Keyword, queryParam?.Targets)
                .Sort(uniqueSort)
                .ToPagedListAsync(
                    queryParam?.Page ?? 1,
                    queryParam?.PageSize ?? 100,
                    cancellationToken
                );
        }

        public async Task<PaginationResponse<TResult>> PagedListAsync<TResult>(
            string functionName,
            object[] parameters,
            string defaultSort,
            QueryParamRequest queryParam,
            Expression<Func<T, TResult>> mappingResult,
            CancellationToken cancellationToken = default
        )
        {
            string uniqueSort = !string.IsNullOrEmpty(queryParam.Sort)
                ? queryParam.Sort
                : defaultSort;
            return await unitOfWork
                .CallPostgreSqlFunction<T>(functionName, parameters)
                .Select(mappingResult)
                .Filter(queryParam.Filter)
                .Search(queryParam?.Keyword, queryParam?.Targets)
                .Sort(uniqueSort)
                .ToPagedListAsync(
                    queryParam?.Page ?? 1,
                    queryParam?.PageSize ?? 100,
                    cancellationToken
                );
        }
    }
}
