using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Reflection;
using System.Text.Json.Serialization;

public static class UniOfWorkExtension
{
    public static T MapReaderToObject<T>(DbDataReader reader)
        where T : new()
    {
        var obj = new T();
        var props = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToList();

        // Tạo từ điển: normalized_column_name → PropertyInfo
        var propMap = props.ToDictionary(
            p => Normalize(GetMappedName(p)),
            p => p,
            StringComparer.OrdinalIgnoreCase
        );

        for (int i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            var normalized = Normalize(columnName);

            if (propMap.TryGetValue(normalized, out var prop) && !reader.IsDBNull(i))
            {
                var value = reader.GetValue(i);
                var converted = ConvertValue(value, prop.PropertyType);
                prop.SetValue(obj, converted);
            }
        }

        return obj;
    }

    // Ưu tiên [Column], sau đó [JsonPropertyName], rồi đến tên gốc
    private static string GetMappedName(PropertyInfo prop)
    {
        return prop.GetCustomAttribute<ColumnAttribute>()?.Name
            ?? prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
            ?? prop.Name;
    }

    // Chuẩn hóa tên: bỏ _, lowercase để map snake_case → PascalCase
    private static string Normalize(string name)
    {
        return name.Replace("_", "").ToLowerInvariant();
    }

    // Chuyển kiểu an toàn (nullable, enum, Guid,...)
    private static object ConvertValue(object value, Type targetType)
    {
        if (value == null || value == DBNull.Value)
            return null;

        // Unwrap nullable
        var actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (actualType.IsEnum)
            return Enum.ToObject(actualType, value);

        if (actualType == typeof(Ulid))
            return Ulid.Parse(value.ToString());

        if (actualType == typeof(Guid))
            return Guid.Parse(value.ToString());

        // Fix DateTime to DateTimeOffset
        if (value is DateTime dt && actualType == typeof(DateTimeOffset))
            return new DateTimeOffset(dt);

        return Convert.ChangeType(value, actualType);
    }
}
