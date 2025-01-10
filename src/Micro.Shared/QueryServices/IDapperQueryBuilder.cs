using Dapper;
using Micro.Shared.Model;

namespace Micro.Shared.QueryServices;
public interface IDapperQueryBuilder
{
    Task<string> BuildQuery<T>(
        QueryParameters? parameters,
        out DynamicParameters dapperParameters,
        string defaultFields = "*", string customQuery = "");
}