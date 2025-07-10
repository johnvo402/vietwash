using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Contracts.RouteResults;

public static class ResultMapping
{
    public static ActionResult<ApiResponse<T>> ToActionResult<T>(this Result<T> result)
        where T : class
    {
        return result.Match(
            onSuccess: data => new OkObjectResult(new ApiResponse<T>(data, Message.SUCCESS)),
            onFailure: error => new ObjectResult(error.ToProblemDetails())
            {
                StatusCode = error.Status,
            }
        );
    }

    public static ActionResult ToCreatedResult(this Result result)
    {
        return result.Match<ActionResult>(
            onSuccess: () =>
                new ObjectResult(
                    new ApiResponse(
                        message: Message.SUCCESS,
                        statusCode: StatusCodes.Status201Created
                    )
                )
                {
                    StatusCode = StatusCodes.Status201Created,
                },
            onFailure: error => new ObjectResult(error.ToProblemDetails())
            {
                StatusCode = error.Status,
            }
        );
    }

    public static ActionResult ToActionResult(this Result result)
    {
        return result.Match<ActionResult>(
            onSuccess: () => new OkObjectResult(new ApiResponse(message: Message.SUCCESS)),
            onFailure: error => new ObjectResult(error.ToProblemDetails())
            {
                StatusCode = error.Status,
            }
        );
    }

    public static ActionResult<ApiResponse<T>> ToCreatedResult<T>(this Result<T> result)
        where T : class
    {
        return result.Match(
            onSuccess: data => new ObjectResult(new ApiResponse<T>(data, Message.SUCCESS)),
            onFailure: error => new ObjectResult(error.ToProblemDetails())
            {
                StatusCode = error.Status,
            }
        );
    }

    public static ActionResult<ApiResponse<T>> ToCreatedResult<T>(
        this Result<T> result,
        object routeValues,
        string route
    )
        where T : class
    {
        return result.Match(
            onSuccess: data => new CreatedAtRouteResult(
                route,
                routeValues,
                new ApiResponse<T>(data, Message.SUCCESS)
            ),
            onFailure: error => new ObjectResult(error.ToProblemDetails())
            {
                StatusCode = error.Status,
            }
        );
    }

    public static ActionResult ToCreatedResult(this Result result, object id, string route)
    {
        return result.Match<ActionResult>(
            onSuccess: () =>
                new CreatedAtRouteResult(route, new { id }, new ApiResponse(Message.SUCCESS)),
            onFailure: error => new ObjectResult(error.ToProblemDetails())
            {
                StatusCode = error.Status,
            }
        );
    }

    public static ActionResult ToNoContentResult(this Result result)
    {
        return result.Match<ActionResult>(
            onSuccess: () => new NoContentResult(),
            onFailure: error => new ObjectResult(error.ToProblemDetails())
            {
                StatusCode = error.Status,
            }
        );
    }

    private static ProblemDetails ToProblemDetails(this ErrorDetails errorDetails) =>
        new()
        {
            Status = errorDetails.Status,
            Title = errorDetails.Title,
            Type = errorDetails.Type,
            Extensions = new Dictionary<string, object?>
            {
                { "ErrorDetail", errorDetails.ErrorMessage },
                { "invalidParams", errorDetails.InvalidParams },
            },
        };
}
