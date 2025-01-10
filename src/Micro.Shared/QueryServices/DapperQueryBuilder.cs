using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;
using Micro.Shared.Model;
using Microsoft.Extensions.Logging;

namespace Micro.Shared.QueryServices;

public class DapperQueryBuilder : IDapperQueryBuilder
{
    private readonly ILogger<DapperQueryBuilder> _logger;
    public DapperQueryBuilder(ILogger<DapperQueryBuilder> logger)
    {
        _logger = logger;
    }
    public Task<string> BuildQuery<T>(
    QueryParameters? parameters,
    out DynamicParameters dapperParameters,
    string defaultFields = "*", string customQuery = "")
    {
        Type type = typeof(T);
        var tableName = ToSnakeCase(type.Name);
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var simpleProperties = properties.Where(p => !typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType)
                                                        || p.PropertyType == typeof(string)
                                                        || p.PropertyType == typeof(Guid));
        var fields = defaultFields == "*"
            ? string.Join(", ", simpleProperties.Select(p => $"[{ToSnakeCase(p.Name)}]"))
            : string.Join(", ", defaultFields.Split(',').Select(f => ToSnakeCase(f.Trim())));

        var queryBuilder = new StringBuilder($"SELECT {fields} FROM [{tableName}]");
        if (!string.IsNullOrWhiteSpace(customQuery))
        {
            queryBuilder = new StringBuilder(customQuery);
        }
        dapperParameters = new DynamicParameters();

        // WHERE condition
        if (parameters?.Where != null)
        {
            if (!string.IsNullOrWhiteSpace(parameters.Where))
            {
                queryBuilder.Append(" WHERE ").Append(parameters.Where);
            }
        }
        if (!string.IsNullOrWhiteSpace(parameters?.GroupBy))
        {
            queryBuilder.Append(" GROUP BY ").Append(parameters.GroupBy);
        }
        // ORDER BY clause
        if (!string.IsNullOrWhiteSpace(parameters?.OrderBy))
        {
            queryBuilder.Append(" ORDER BY ").Append(parameters.OrderBy);
        }

        // OFFSET and LIMIT
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        if (parameters.Offset.HasValue && parameters.Limit.HasValue)
        {
            queryBuilder.Append(" OFFSET ").Append(parameters.Offset.Value).Append(" ROWS");
            queryBuilder.Append(" FETCH NEXT ").Append(parameters.Limit.Value).Append(" ROWS ONLY");
        }
        else if (parameters.Limit.HasValue)
        {
            queryBuilder.Append(" FETCH FIRST ").Append(parameters.Limit.Value).Append(" ROWS ONLY");
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        _logger.LogInformation($"CreatedAt: {DateTimeOffset.Now} Query: {queryBuilder}");
        return Task.FromResult(queryBuilder.ToString());
    }

    // Method to convert PascalCase to snake_case
    public static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        var builder = new StringBuilder();
        foreach (char c in name)
        {
            if (char.IsUpper(c))
            {
                if (builder.Length > 0)
                {
                    builder.Append('_');
                }
                builder.Append(char.ToLower(c));
            }
            else
            {
                builder.Append(c);
            }
        }
        return builder.ToString();
    }

}


