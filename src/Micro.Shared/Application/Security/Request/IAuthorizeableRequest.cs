using MediatR;

namespace Micro.Shared.Application.Security.Request;
public interface IAuthorizeableRequest<T> : IRequest<T>
{
}