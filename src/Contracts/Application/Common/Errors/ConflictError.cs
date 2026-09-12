using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Errors;

public class ConflictError(string title, MessageResult messageResult)
    : ErrorDetails(title, messageResult, nameof(ConflictError), StatusCodes.Status409Conflict);
