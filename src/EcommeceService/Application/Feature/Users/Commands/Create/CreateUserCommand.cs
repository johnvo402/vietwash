using Application.Features.Common.Projections.Users;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Features.Users.Commands.Create;

public class CreateUserCommand : QueueBasePayload<UserModel>, IRequest<QueueResponse<CreateUserCommand>>;
