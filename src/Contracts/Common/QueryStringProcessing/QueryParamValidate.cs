using System.Reflection;
using Application.Common.Errors;
using Contracts.Application.Common.Extensions;
using Contracts.Common.Messages;
using Contracts.Dtos.Requests;
using Contracts.Extensions;
using Contracts.Extensions.Reflections;
using Serilog;
using Shared.Kernel.Extensions;
using StringExtension = Contracts.Application.Common.Extensions.StringExtension;

namespace Contracts.Common.QueryStringProcessing;

public static partial class QueryParamValidate
{
    private const string Message = "Your request parameters didn't validate.";

    public static ValidationRequestResult<T, BadRequestError> ValidateQuery<T>(this T request)
        where T : QueryParamRequest
    {
        if (!string.IsNullOrWhiteSpace(request.Before) && !string.IsNullOrWhiteSpace(request.After))
        {
            return new(
                Error: new BadRequestError(
                    Message,
                    Messager
                        .Create<QueryParamRequest>("QueryParam")
                        .Property("Cursor")
                        .Message(MessageType.Redundant)
                        .Build()
                )
            );
        }

        return new(request);
    }

    public static ValidationRequestResult<TRequest, BadRequestError> ValidateFilter<
        TRequest,
        TResponse
    >(this TRequest request)
        where TResponse : class
        where TRequest : QueryParamRequest
    {
        if (request.OriginFilters == null || request.OriginFilters.Length == 0)
        {
            return new(request);
        }

        List<QueryResult> queries =
        [
            .. StringExtension.TransformStringQuery(request.OriginFilters),
        ];
        int length = queries.Count;

        for (int i = 0; i < length; i++)
        {
            QueryResult query = queries[i];
            Log.Information(
                "Processing query: {CleanKey}, Value: {Value}",
                string.Join(".", query.CleanKey),
                query.Value
            );

            // Kiểm tra array operator ($and, $or, $in, $between)
            if (!ValidateArrayOperator(query.CleanKey))
            {
                Log.Error(
                    "ValidateArrayOperator failed for {CleanKey}",
                    string.Join(".", query.CleanKey)
                );
                return new(
                    Error: new BadRequestError(
                        Message,
                        Messager
                            .Create<QueryParamRequest>("QueryParam")
                            .Property(x => x.Filter!)
                            .Message(MessageType.Missing)
                            .ObjectName("ArrayIndex")
                            .Build()
                    )
                );
            }

            // Kiểm tra index của array operator bắt đầu từ 0
            if (i == 0 && !ValidateArrayOperatorInvalidIndex(query.CleanKey))
            {
                Log.Error(
                    "ValidateArrayOperatorInvalidIndex failed for {CleanKey}",
                    string.Join(".", query.CleanKey)
                );
                return new(
                    Error: new BadRequestError(
                        Message,
                        Messager
                            .Create<QueryParamRequest>("QueryParam")
                            .Property(x => x.Filter!)
                            .Message(MessageType.Valid)
                            .Negative()
                            .ObjectName("ArrayIndex")
                            .Build()
                    )
                );
            }

            // Kiểm tra thiếu operator
            if (!ValidateLackOfOperator(query.CleanKey))
            {
                Log.Error(
                    "ValidateLackOfOperator failed for {CleanKey}",
                    string.Join(".", query.CleanKey)
                );
                return new(
                    Error: new BadRequestError(
                        Message,
                        Messager
                            .Create<QueryParamRequest>("QueryParam")
                            .Property(x => x.Filter!)
                            .Message(MessageType.Missing)
                            .ObjectName("Operator")
                            .Build()
                    )
                );
            }

            // Kiểm tra thiếu element sau logical operator
            if (LackOfElementInArrayOperator(query.CleanKey))
            {
                Log.Error(
                    "LackOfElementInArrayOperator failed for {CleanKey}",
                    string.Join(".", query.CleanKey)
                );
                return new(
                    Error: new BadRequestError(
                        Message,
                        Messager
                            .Create<QueryParamRequest>("QueryParam")
                            .Property(x => x.Filter!)
                            .Message(MessageType.Missing)
                            .ObjectName("Element")
                            .Build()
                    )
                );
            }

            // Lấy danh sách properties
            IEnumerable<string> properties = query.CleanKey.Where(x =>
                string.Compare(x, "$or", StringComparison.OrdinalIgnoreCase) != 0
                && string.Compare(x, "$and", StringComparison.OrdinalIgnoreCase) != 0
                && !x.IsDigit()
                && !validOperators.Contains(x.ToLower())
            );

            // Kiểm tra thiếu property
            if (!properties.Any())
            {
                Log.Error("No properties found for {CleanKey}", string.Join(".", query.CleanKey));
                return new(
                    Error: new BadRequestError(
                        Message,
                        Messager
                            .Create<QueryParamRequest>("QueryParam")
                            .Property(x => x.Filter!)
                            .Message(MessageType.Missing)
                            .ObjectName("Property")
                            .Build()
                    )
                );
            }

            // Kiểm tra thuộc tính trong TResponse
            Type type = typeof(TResponse);
            PropertyInfo propertyInfo;
            try
            {
                propertyInfo = type.GetNestedPropertyInfo(string.Join(".", properties));
            }
            catch (Exception ex)
            {
                Log.Error(
                    "GetNestedPropertyInfo failed for {Properties}: {Error}",
                    string.Join(".", properties),
                    ex.Message
                );
                return new(
                    Error: new BadRequestError(
                        Message,
                        Messager
                            .Create<QueryParamRequest>("QueryParam")
                            .Property(x => x.Filter!)
                            .Message(MessageType.Missing)
                            .ObjectName("Property")
                            .Build()
                    )
                );
            }

            Type[] arguments = propertyInfo.PropertyType.GetGenericArguments();
            Type nullableType = arguments.Length > 0 ? arguments[0] : propertyInfo.PropertyType;

            // Kiểm tra giá trị enum
            if (nullableType.IsEnum)
            {
                if (
                    string.IsNullOrWhiteSpace(query.Value)
                    || !Enum.IsDefined(nullableType, query.Value)
                )
                {
                    Log.Error(
                        "Invalid enum value for {Property}: {Value}",
                        string.Join(".", properties),
                        query.Value
                    );
                    return new(
                        Error: new BadRequestError(
                            Message,
                            Messager
                                .Create<QueryParamRequest>("QueryParam")
                                .Property("FilterValue")
                                .Message(MessageType.Matching)
                                .Negative()
                                .ObjectName("Enum")
                                .Build()
                        )
                    );
                }
                // Thay thế tên enum bằng giá trị số
                string originalValue = query.Value;
                object enumValue = Enum.Parse(nullableType, query.Value);
                string newValue = Convert.ToInt64(enumValue).ToString(); // Hỗ trợ mọi kiểu cơ bản
                queries[i] = query with { Value = newValue };
                Log.Information(
                    "Converted enum {OriginalValue} to {NewValue} for {Property}",
                    originalValue,
                    newValue,
                    string.Join(".", properties)
                );
            }
            // Kiểm tra giá trị numeric
            else if (IsNumericType(nullableType) && query.Value?.All(char.IsDigit) == false)
            {
                Log.Error(
                    "Invalid numeric value for {Property}: {Value}",
                    string.Join(".", properties),
                    query.Value
                );
                return new(
                    Error: new BadRequestError(
                        Message,
                        Messager
                            .Create<QueryParamRequest>("QueryParam")
                            .Property("FilterValue")
                            .Message(MessageType.Matching)
                            .Negative()
                            .ObjectName("Integer")
                            .Build()
                    )
                );
            }

            // Kiểm tra giá trị datetime
            if (
                (nullableType == typeof(DateTime) && !DateTime.TryParse(query.Value, out _))
                || (
                    nullableType == typeof(DateTimeOffset)
                    && !DateTimeOffset.TryParse(query.Value, out _)
                )
            )
            {
                Log.Error(
                    "Invalid datetime value for {Property}: {Value}",
                    string.Join(".", properties),
                    query.Value
                );
                return new(
                    Error: new BadRequestError(
                        Message,
                        Messager
                            .Create<QueryParamRequest>("QueryParam")
                            .Property("FilterValue")
                            .Message(MessageType.Matching)
                            .Negative()
                            .ObjectName("Datetime")
                            .Build()
                    )
                );
            }

            // Kiểm tra giá trị Ulid
            if ((nullableType == typeof(Ulid)) && !Ulid.TryParse(query.Value, out _))
            {
                Log.Error(
                    "Invalid Ulid value for {Property}: {Value}",
                    string.Join(".", properties),
                    query.Value
                );
                return new(
                    Error: new BadRequestError(
                        Message,
                        Messager
                            .Create<QueryParamRequest>("QueryParam")
                            .Property("FilterValue")
                            .Message(MessageType.Matching)
                            .Negative()
                            .ObjectName("Ulid")
                            .Build()
                    )
                );
            }
        }

        // Kiểm tra $between operator
        if (!ValidateBetweenAndInOperator("$between", queries))
        {
            Log.Error("Invalid $between operator format");
            return new(
                Error: new BadRequestError(
                    Message,
                    Messager
                        .Create<QueryParamRequest>("QueryParam")
                        .Property(x => x.Filter!)
                        .Message(MessageType.Valid)
                        .ObjectName("BetweenOperator")
                        .Negative()
                        .Build()
                )
            );
        }

        // Kiểm tra $in operator
        if (!ValidateBetweenAndInOperator("$in", queries))
        {
            Log.Error("Invalid $in operator format");
            return new(
                Error: new BadRequestError(
                    Message,
                    Messager
                        .Create<QueryParamRequest>("QueryParam")
                        .Property(x => x.Filter!)
                        .Message(MessageType.Valid)
                        .ObjectName("InOperator")
                        .Negative()
                        .Build()
                )
            );
        }

        // Kiểm tra trùng lặp filter
        var trimQueries = queries.Select(x => string.Join(".", x.CleanKey));
        if (trimQueries.Distinct().Count() != queries.Count)
        {
            Log.Error("Duplicated filter elements found");
            return new(
                Error: new BadRequestError(
                    Message,
                    Messager
                        .Create<QueryParamRequest>("QueryParam")
                        .Property("FilterElement")
                        .Message(MessageType.Unique)
                        .Negative()
                        .Build()
                )
            );
        }

        // Gán filter đã xử lý
        request.Filter = StringExtension.Parse(queries);
        Log.Information(
            "Filter has been bound {filter}",
            SerializerExtension.Serialize(request.Filter!).StringJson
        );

        return new(request);
    }

    private static bool ValidateBetweenAndInOperator(
        string operation,
        IEnumerable<QueryResult> queries
    )
    {
        var betweenOperators = queries.Where(x => x.CleanKey.Contains(operation)).ToList();
        if (!betweenOperators.Any())
        {
            return true; // Không có operator, hợp lệ
        }

        var betweenOperatorsGroup = betweenOperators
            .Select(betweenOperator =>
            {
                int betweenIndex = betweenOperator.CleanKey.IndexOf(operation);
                if (betweenIndex <= 0)
                {
                    throw new InvalidOperationException("Invalid format of cleanKey.");
                }
                int index = betweenIndex - 1;
                string key = string.Join(
                    ".",
                    betweenOperator
                        .CleanKey.Skip(index)
                        .Take(betweenOperator.CleanKey.Count - betweenIndex)
                );

                if (!int.TryParse(betweenOperator.CleanKey.Last(), out int indexValue))
                {
                    return new { key, indexValue = -1 };
                }

                if (
                    betweenOperator.CleanKey.Contains("$and")
                    && int.TryParse(
                        betweenOperator.CleanKey[betweenOperator.CleanKey.IndexOf("$and") + 1],
                        out int andIndex
                    )
                )
                {
                    return new { key = $"$and.{andIndex}.{key}", indexValue };
                }
                if (
                    betweenOperator.CleanKey.Contains("$or")
                    && int.TryParse(
                        betweenOperator.CleanKey[betweenOperator.CleanKey.IndexOf("$or") + 1],
                        out int orIndex
                    )
                )
                {
                    return new { key = $"$or.{orIndex}.{key}", indexValue };
                }
                return new { key, indexValue };
            })
            .GroupBy(x => x.key)
            .Select(x => new { x.Key, values = x.Select(x => x.indexValue).ToList() })
            .ToList();

        return betweenOperatorsGroup.Count == 1
                && betweenOperatorsGroup[0].values.OrderBy(x => x).SequenceEqual(Enumerable.Range(0, betweenOperatorsGroup[0].values.Count));
    }

    private static bool IsNumericType(Type type)
    {
        return Type.GetTypeCode(type) switch
        {
            TypeCode.Byte
            or TypeCode.SByte
            or TypeCode.UInt16
            or TypeCode.UInt32
            or TypeCode.UInt64
            or TypeCode.Int16
            or TypeCode.Int32
            or TypeCode.Int64
            or TypeCode.Decimal
            or TypeCode.Double
            or TypeCode.Single => true,
            _ => false,
        };
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="input"></param>
    /// <returns>true if any element is after $and,$or,$in,$between isn't degit otherwise false</returns>
    private static bool ValidateArrayOperator(List<string> input)
    {
        var validOperators = new HashSet<string> { "$and", "$or", "$in", "$between" };
        for (int i = 0; i < input.Count; i++)
        {
            if (!validOperators.Contains(input[i]))
            {
                continue;
            }
            if (i + 1 >= input.Count)
            {
                continue;
            }
            if (!input[i + 1].All(char.IsDigit))
            {
                return false;
            }
        }
        if (input[^1] == validOperators.Last() || input[^1] == "$in")
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// check if array operator has invalid index like $and[1][firstName], index must start with 0.
    /// </summary>
    /// <param name="input"></param>
    /// /// <returns></returns>
    private static bool ValidateArrayOperatorInvalidIndex(List<string> input)
    {
        var validOperators = new HashSet<string> { "$and", "$or", "$in", "$between" };
        for (int i = 0; i < input.Count; i++)
        {
            if (!validOperators.Contains(input[i]))
            {
                continue;
            }
            if (i + 1 >= input.Count)
            {
                continue;
            }
            string theNextItem = input[i + 1];
            if (!theNextItem.All(char.IsDigit) || int.Parse(theNextItem) != 0)
            {
                return false;
            }
        }
        if (input[^1] == validOperators.Last() || input[^1] == "$in")
        {
            return false;
        }
        return true;
    }

    private static bool ValidateLackOfOperator(List<string> input)
    {
        Stack<string> inputs = new(input);
        string last = inputs.Pop();
        string preLast = inputs.Pop();
        if (arrayOperators.Contains(preLast.ToLower()))
        {
            return true;
        }
        return validOperators.Contains(last.ToLower());
    }

    private static bool LackOfElementInArrayOperator(List<string> input)
    {
        Stack<string> inputs = new(input);
        string last = inputs.Pop();
        string preLast = inputs.Pop();
        return logicalOperators.Contains(preLast.ToLower()) && last.All(char.IsDigit);
    }

    private static readonly string[] validOperators =
    [
        "$eq",
        "$eqi",
        "$ne",
        "$nei",
        "$in",
        "$notin",
        "$lt",
        "$lte",
        "$gt",
        "$gte",
        "$between",
        "$notcontains",
        "$notcontainsi",
        "$contains",
        "$containsi",
        "$startswith",
        "$endswith",
    ];

    private static readonly string[] arrayOperators = ["$in", "$between"];
    private static readonly string[] logicalOperators = ["$and", "$or"];
}
