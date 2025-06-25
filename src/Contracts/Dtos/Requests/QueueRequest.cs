namespace Contracts.Dtos.Requests;

public class PubSubRequest<T>
{
    public Guid PayloadId { get; set; }

    public T? Payload { get; set; }
}
