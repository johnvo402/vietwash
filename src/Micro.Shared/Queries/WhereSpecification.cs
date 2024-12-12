using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.Extensions.Logging;
namespace Micro.Shared.Queries
{
    public class WhereSpecification<T> : IQuerySpecification<T>
    {
        private readonly Dictionary<string, Dictionary<string, string>>? _conditions;
        public WhereSpecification(Dictionary<string, Dictionary<string, string>>? conditions)
        {
            _conditions = conditions;
        }

        public IQueryable<T> Apply(IQueryable<T> query)
        {
            if (_conditions == null)
                return query;

            Dictionary<string, Dictionary<string, string>>? conditions;
            try
            {
                conditions = _conditions;
            }
            catch
            {
                return query;
            }

            if (conditions == null || !conditions.Any())
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            var properties = typeof(T).GetProperties()
                .ToDictionary(p => p.Name.ToLower(), p => p);

            foreach (var condition in conditions)
            {
                var key = condition.Key.ToLower();
                var value = condition.Value;

                if (!properties.ContainsKey(key)) continue;

                var propertyInfo = properties[key];
                var prop = Expression.Property(parameter, propertyInfo);

                Expression? filterExpression = null;

                if (value is Dictionary<string, string> subConditions)
                {
                    foreach (var subCondition in subConditions)
                    {
                        filterExpression = GenerateExpression(prop, subCondition.Key, subCondition.Value, propertyInfo.PropertyType);
                    }
                }

                if (filterExpression != null)
                {
                    var lambda = Expression.Lambda<Func<T, bool>>(filterExpression, parameter);
                    query = query.Where(lambda);
                }
            }

            return query;
        }

        private Expression? GenerateExpression(Expression prop, string operatorKey, object conditionValue, Type propertyType)
        {
            Expression? filterExpression = null;

            // Chuyển đổi giá trị một cách an toàn
            object? convertedValue = null;
            if (conditionValue != null && operatorKey != Operator.IS_NULL)
            {
                try
                {
                    // Xử lý JsonElement
                    if (conditionValue is JsonElement jsonElement)
                    {
                        switch (jsonElement.ValueKind)
                        {
                            case JsonValueKind.Number:
                                if (propertyType == typeof(int))
                                    convertedValue = jsonElement.GetInt32();
                                else if (propertyType == typeof(long))
                                    convertedValue = jsonElement.GetInt64();
                                else if (propertyType == typeof(decimal))
                                    convertedValue = jsonElement.GetDecimal();
                                else if (propertyType == typeof(double))
                                    convertedValue = jsonElement.GetDouble();
                                break;
                            case JsonValueKind.String:
                                convertedValue = jsonElement.GetString();
                                break;
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                convertedValue = jsonElement.GetBoolean();
                                break;
                        }
                    }

                    // Nếu chưa convert được qua JsonElement thì thử các cách khác
                    if (convertedValue == null)
                    {
                        if (propertyType == typeof(Guid))
                        {
                            convertedValue = Guid.Parse(conditionValue.ToString() ?? "");
                        }
                        else if (propertyType.IsEnum)
                        {
                            convertedValue = Enum.Parse(propertyType, conditionValue.ToString() ?? "");
                        }
                        else if (propertyType == typeof(DateTime))
                        {
                            convertedValue = DateTime.Parse(conditionValue.ToString() ?? "");
                        }
                        else
                        {
                            convertedValue = Convert.ChangeType(conditionValue, propertyType);
                        }
                    }
                }
                catch
                {
                    return null; // Trả về null nếu không thể chuyển đổi
                }
            }

            switch (operatorKey.ToLower())
            {
                case Operator.NEQ:
                    filterExpression = Expression.NotEqual(prop, Expression.Constant(convertedValue, propertyType));
                    break;

                case Operator.EQ:
                    filterExpression = Expression.Equal(prop, Expression.Constant(convertedValue, propertyType));
                    break;

                case Operator.IN:
                    if (conditionValue is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                    {
                        var list = new List<object>();
                        foreach (var element in jsonElement.EnumerateArray())
                        {
                            try
                            {
                                var convertedItem = propertyType == typeof(Guid)
                                    ? Guid.Parse(element.GetString() ?? "")
                                    : Convert.ChangeType(element.GetString(), propertyType);
                                if (convertedItem != null)
                                {
                                    list.Add(convertedItem);
                                }
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        var listType = typeof(List<>).MakeGenericType(propertyType);
                        var typedList = Activator.CreateInstance(listType);
                        var addMethod = listType.GetMethod("Add") 
                            ?? throw new InvalidOperationException("Add method not found");
                        foreach (var item in list)
                        {
                            addMethod.Invoke(typedList, new[] { item });
                        }

                        var listExpr = Expression.Constant(typedList, listType);
                        var containsMethod = typeof(Enumerable)
                            .GetMethods()
                            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                            .MakeGenericMethod(propertyType);

                        filterExpression = Expression.Call(containsMethod, listExpr, prop);
                    }
                    break;

                case Operator.IS_NULL:
                    filterExpression = Expression.Equal(prop, Expression.Constant(null, propertyType));
                    break;

                case Operator.LIKE:
                    if (propertyType == typeof(string))
                    {
                        var pattern = convertedValue?.ToString();
                        if (pattern != null)
                        {
                            // Chuyển đổi pattern từ SQL LIKE sang Regex
                            pattern = pattern.Replace("%", ".*").Replace("_", ".");
                            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) })
                                ?? throw new InvalidOperationException("Contains method not found");
                            var searchValue = Expression.Constant(pattern.Replace(".*", ""));
                            filterExpression = Expression.Call(prop, method, searchValue);
                        }
                    }
                    break;

                case Operator.ILIKE:
                    if (propertyType == typeof(string))
                    {
                        var pattern = convertedValue?.ToString();
                        if (pattern != null)
                        {
                            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes) 
                                ?? throw new InvalidOperationException("ToLower method not found");
                            var lowerProperty = Expression.Call(prop, toLowerMethod);
                            
                            // For ILIKE, wrap the pattern with % if not already present
                            var lowerPattern = pattern.ToLower();
                            if (!lowerPattern.StartsWith("%")) lowerPattern = "%" + lowerPattern;
                            if (!lowerPattern.EndsWith("%")) lowerPattern += "%";

                            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })
                                ?? throw new InvalidOperationException("Contains method not found");
                            // Remove the % wildcards for the actual contains check
                            var searchValue = Expression.Constant(lowerPattern.Trim('%'));
                            filterExpression = Expression.Call(lowerProperty, containsMethod, searchValue);
                        }
                    }
                    break;
            }

            return filterExpression;
        }
    }
}
public static class Operator
{
    public const string NEQ = "_neq";
    public const string EQ = "_eq";
    public const string IN = "_in";
    public const string IS_NULL = "_is_null";
    public const string LIKE = "_like";
    public const string ILIKE = "_ilike";
}
