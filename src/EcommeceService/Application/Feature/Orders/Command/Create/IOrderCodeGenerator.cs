using Contracts.Utils;

namespace Application.Feature.Orders.Command.Create;

public interface IOrderCodeGenerator
{
    string Generate();
}

public sealed class OrderCodeGenerator : IOrderCodeGenerator
{
    public string Generate() => Generator.GenerateCode("OD", 6);
}

public interface IOrderCodeCollisionDetector
{
    bool IsOrderCodeCollision(Exception exception);
}
