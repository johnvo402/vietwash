using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dapper;

namespace Micro.Shared.Infrastructure.Policies;
public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
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

public class SnakeCaseTypeMap : SqlMapper.ITypeMap
{
    private readonly Type _type;

    public SnakeCaseTypeMap(Type type)
    {
        _type = type;
    }

    public ConstructorInfo FindConstructor(string[] names, Type[] types)
    {
        var constructor = _type.GetConstructor(types);
        if (constructor == null)
        {
            throw new InvalidOperationException($"Constructor not found for type {_type.Name}");
        }
        return constructor;
    }

    public ConstructorInfo FindExplicitConstructor()
    {
        var constructor = _type.GetConstructors().FirstOrDefault();
        if (constructor == null)
        {
            throw new InvalidOperationException($"No public constructor found for type {_type.Name}");
        }
        return constructor;
    }

    public SqlMapper.IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
    {
        throw new NotImplementedException("Constructor parameter mapping is not implemented.");
    }

    public SqlMapper.IMemberMap? GetMember(string columnName)
    {
        var propertyName = ToPascalCase(columnName);
        var property = _type.GetProperties().FirstOrDefault(p => p.Name == propertyName);

        if (property != null)
        {
            return new SimpleMemberMap(property);
        }

        return null;
    }

    private string ToPascalCase(string str)
    {
        return Regex.Replace(str, @"_([a-z])", match => match.Groups[1].Value.ToUpper());
    }

}

public class SimpleMemberMap : SqlMapper.IMemberMap
{
    private readonly PropertyInfo _property;

    public SimpleMemberMap(PropertyInfo property)
    {
        _property = property;
    }

    public PropertyInfo Property => _property;

    public string ColumnName => ToSnakeCase(_property.Name);

    public Type MemberType => _property.PropertyType;

    public FieldInfo? Field => null; 

    public ParameterInfo? Parameter => null;

    private string ToSnakeCase(string str)
    {
        return Regex.Replace(str, @"([a-z])([A-Z])", "$1_$2").ToLower();
    }
}