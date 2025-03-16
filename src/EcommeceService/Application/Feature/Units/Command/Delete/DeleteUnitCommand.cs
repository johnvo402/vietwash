using Mediator;


namespace Application.Feature.Units.Command.Delete
{
	public record DeleteUnitCommand(Ulid UnitId) : IRequest;

}
