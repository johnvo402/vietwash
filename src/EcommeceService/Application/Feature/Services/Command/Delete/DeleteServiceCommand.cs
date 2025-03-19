using Mediator;

namespace Application.Feature.Services.Command.Delete;

public record DeleteServiceCommand(Ulid ServiceId) : IRequest;
