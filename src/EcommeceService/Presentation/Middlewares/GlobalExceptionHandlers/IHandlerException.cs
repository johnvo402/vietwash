using JohnChum.SharedKernel.Domain.Exceptions;

namespace Presentation.Middlewares.GlobalExceptionHandlers;

public interface IHandlerException<T> : IHandlerException
    where T : CustomException
{
}

public interface IHandlerException
{
    Task Handle(HttpContext httpContext, Exception ex);
}