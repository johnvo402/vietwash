using System.Data;
using AuthService.Application.Interfaces;
using AuthService.Domain.Users.Entity;
using Dapper;
using ErrorOr;
using MediatR;
using Micro.Shared.Model;
using Micro.Shared.QueryServices;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Users.Queries.GetUserQueries;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, ErrorOr<ApiResponseQuery<User>>>
{
    private readonly IDbConnection _dbConnection;
    private readonly IDapperQueryBuilder _dapperQueryBuilder;

    public GetUserQueryHandler(IDbConnection dbConnection, IDapperQueryBuilder dapperQueryBuilder)
    {
        _dbConnection = dbConnection;
        _dapperQueryBuilder = dapperQueryBuilder;
    }


    public async Task<ErrorOr<ApiResponseQuery<User>>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        if (request?.request == null)
        {
            return Error.Failure("Invalid request.");
        }

        try
        {
            string query = await _dapperQueryBuilder.BuildQuery<User>(request.request, out DynamicParameters dapperParams);
            var data = await _dbConnection.QueryAsync<User>(query, dapperParams);
            return new ApiResponseQuery<User>
            {
                Data = data?.ToList() ?? new List<User>(),
                Total = data?.Count() ?? 0
            };
        }
        catch (SqlException ex)
        {
            return Error.Failure("Database query failed." + ex.Message);
        }
        catch (Exception ex)
        {
            return Error.Failure("An unexpected error occurred." + ex.Message);
        }
    }
}
