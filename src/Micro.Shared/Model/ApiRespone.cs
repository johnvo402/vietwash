namespace Micro.Shared.Model;
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Value { get; set; }
}

public class ApiResponseQuery<T>
{
    public IEnumerable<T>? Data { get; set; }
    public int Total { get; set; }
}