namespace Micro.Shared.Model;
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Value { get; set; }
}