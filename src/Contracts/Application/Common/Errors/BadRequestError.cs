using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Errors;

public class BadRequestError(string title, MessageResult messageResult)
    : ErrorDetails(title, messageResult, nameof(BadRequestError), StatusCodes.Status400BadRequest);
