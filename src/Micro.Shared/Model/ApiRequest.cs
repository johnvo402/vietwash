using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Micro.Shared.Model;

public class ApiRequestPost<T>
{
    public T? Object { get; set; }
    public List<T>? Objects { get; set; }
}
public class ApiRequestPut<T>
{
    public Guid Id { get; set; } = Guid.Empty;
    public T? Object { get; set; }
}


public class QueryParameters
{
    public string? Where { get; set; }
    public string? OrderBy { get; set; }
    public int? Offset { get; set; }
    public int? Limit { get; set; }
    public string? GroupBy { get; set; }
}
