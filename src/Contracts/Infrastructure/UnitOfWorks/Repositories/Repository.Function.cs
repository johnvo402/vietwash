using System.Text.RegularExpressions;
using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Npgsql;

namespace Contracts.Infrastructure.UnitOfWorks.Repositories
{
    public partial class RepositoryFunction<T>(IUnitOfWork unitOfWork) : IRepositoryFunction<T>
        where T : new()
    {
        private static readonly Regex ColumnNameRegex = new(
            @"^[a-zA-Z_][a-zA-Z0-9_]*$",
            RegexOptions.Compiled
        );

        public async Task<PaginationResponse<T>> ExecuteFunctionWithPagingAsync(
            string functionName,
            IDictionary<string, object?> parameters,
            string? sort,
            int page,
            int pageSize,
            string defaultSort,
            CancellationToken cancellationToken = default
        )
        {
            var paramList = parameters
                .Select(p => new NpgsqlParameter(p.Key, p.Value ?? DBNull.Value))
                .ToList();

            var sortClause = BuildOrderByClauseMultiSort(sort, defaultSort);

            var limitParam = new NpgsqlParameter("limit", pageSize);
            var offsetParam = new NpgsqlParameter("offset", (page - 1) * pageSize);

            var functionParamNames = string.Join(", ", parameters.Keys.Select(k => $"@{k}"));

            var query =
                $@"
            SELECT * FROM {functionName}({functionParamNames})
            ORDER BY {sortClause}
            LIMIT @limit OFFSET @offset;
        ";

            var allParams = paramList.Concat([limitParam, offsetParam]).ToArray();

            var results = await unitOfWork.ExecuteSqlQueryAsync<T>(
                query,
                allParams,
                cancellationToken
            );

            var countQuery =
                $@"
            SELECT COUNT(*) FROM {functionName}({functionParamNames});
        ";

            var totalCount = await unitOfWork.ExecuteScalarAsync<long>(
                countQuery,
                paramList.ToArray(),
                cancellationToken
            );

            var totalPage = (int)Math.Ceiling((double)totalCount / pageSize);

            bool hasNext = totalPage > page;
            bool hasPre = totalPage <= page;

            return new PaginationResponse<T>(results, totalPage, page, pageSize);
        }

        private static string BuildOrderByClauseMultiSort(string? sortInput, string defaultSort)
        {
            if (string.IsNullOrWhiteSpace(sortInput))
                return defaultSort;

            var sortItems = sortInput
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s =>
                {
                    var parts = s.Split(
                        ':',
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                    );
                    if (parts.Length != 2)
                        return null;

                    var column = parts[0];
                    var direction = parts[1].ToUpperInvariant();

                    if (!ColumnNameRegex.IsMatch(column))
                        return null;

                    if (direction != "ASC" && direction != "DESC")
                        direction = "DESC";

                    return $"{column} {direction}";
                })
                .Where(clause => clause != null)
                .ToList();

            return sortItems.Count > 0 ? string.Join(", ", sortItems) : defaultSort;
        }
    }
}
