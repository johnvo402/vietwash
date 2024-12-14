using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Micro.Shared.Model;

public class ApiRequestPost<T>
{
    [JsonPropertyName("object")]
    public T? Object { get; set; }
    [JsonPropertyName("objects")]
    public List<T>? Objects { get; set; }
}
public class ApiRequestPut<T>
{
    [JsonPropertyName("id")]
    [Required]
    public Guid Id { get; set; }
    [JsonPropertyName("object")]
    public T? Object { get; set; }
    [JsonPropertyName("objects")]
    public List<T>? Objects { get; set; }
}
