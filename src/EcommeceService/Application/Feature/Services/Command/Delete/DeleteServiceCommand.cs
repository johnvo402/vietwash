using Mediator;

namespace Application.Feature.Services.Command.Delete;

public record DeleteServiceCommand(long ServiceId) : IRequest;
