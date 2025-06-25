using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Errors;

public class NotFoundError(string title, MessageResult message)
    : ErrorDetails(title, message, nameof(NotFoundError), StatusCodes.Status404NotFound);
