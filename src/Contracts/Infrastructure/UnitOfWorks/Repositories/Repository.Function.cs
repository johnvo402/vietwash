using System.Text.RegularExpressions;
using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Extensions.QueryExtensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Contracts.Infrastructure.UnitOfWorks.Repositories
{
    public partial class RepositoryFunction<T>(IUnitOfWork unitOfWork) : IRepositoryFunction<T>
        where T : class
    {
        public async Task<PaginationResponse<T>> PagedListFunctionAsync(
            string functionName,
            object[] functionParameters,
            string defaultSort,
            QueryParamRequest queryParam,
            CancellationToken cancellationToken = default
        )
        {
            Search? search = queryParam.Search;
            string uniqueSort = !string.IsNullOrEmpty(queryParam.Sort)
                ? queryParam.Sort
                : defaultSort;
            return await unitOfWork
                .CallPostgreSqlFunction<T>(functionName, functionParameters)
                .Filter(queryParam.DynamicFilter)
                .Search(search?.Keyword, search?.Targets)
                .Sort(uniqueSort)
                .ToPagedListAsync(queryParam.Page, queryParam.PageSize, cancellationToken);
        }
    }
}
