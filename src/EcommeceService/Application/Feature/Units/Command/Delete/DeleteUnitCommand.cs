using Mediator;


namespace Application.Feature.Units.Command.Delete
{
	public record DeleteUnitCommand(long UnitId) : IRequest;

}
