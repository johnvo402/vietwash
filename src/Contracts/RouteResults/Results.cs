using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Contracts.ApiWrapper;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Contracts.RouteResults;

public static class Results
{
    public static ActionResult<ApiResponse<T>> Ok200<T>(this ControllerBase controller, T data) =>
        controller.ToActionResult(new Result<ApiResponse<T>>(new ApiResponse<T>(data, Message.SUCCESS, StatusCodes.Status200OK)));

    public static ActionResult<ApiResponse<T>> Created201<T>(this ControllerBase controller, string routeName, long id, T? data = default) =>
        controller.CreatedAtRoute(routeName, new { id }, data);
    public static ActionResult<ApiResponse<T>> Created201<T>(this ControllerBase controller, T data)
    {
        return controller.Created(string.Empty, data);
    }

    public static ActionResult<ApiResponse<Unit>> Created201(this ControllerBase controller) =>
         controller.ToActionResult(new Result<ApiResponse<Unit>>(new ApiResponse<Unit>(Unit.Value, Message.SUCCESS, StatusCodes.Status201Created)));

    public static ActionResult NoContent204(this ControllerBase controller) =>
        controller.NoContent();
}
