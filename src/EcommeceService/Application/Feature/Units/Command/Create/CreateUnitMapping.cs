using Domain.Aggregates.Services;

namespace Application.Feature.Units.Command.Create
{
    public static class CreateUnitMapping
    {
        public static Unit ToUnit(this CreateUnitCommand command)
        {
            return new Unit(command.Name, command.Status);
        }
    }
}
