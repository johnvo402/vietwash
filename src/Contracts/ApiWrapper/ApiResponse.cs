using Microsoft.AspNetCore.Http;

namespace Contracts.ApiWrapper;

public class ApiResponse<T> : ApiBaseResponse
    where T : class
{
    public T? Results { get; set; }

    public ApiResponse() { }

    public ApiResponse(string message, int? statusCode = StatusCodes.Status200OK)
    {
        Results = null;

        Status = statusCode!.Value;

        Message = message;
    }

    public ApiResponse(T? result, string message, int? statusCode = StatusCodes.Status200OK)
    {
        Results = result;

        Status = statusCode!.Value;

        Message = message;
    }
}

public class ApiResponse : ApiBaseResponse
{
    public ApiResponse() { }

    public ApiResponse(string message, int? statusCode = StatusCodes.Status200OK)
    {
        Status = statusCode!.Value;

        Message = message;
    }
}
