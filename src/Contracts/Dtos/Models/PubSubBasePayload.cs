namespace Contracts.Dtos.Models
{
    public class PubSubBasePayload<T>
    {
        public Guid PayloadId { get; set; } = Guid.NewGuid();

        public T? Payload { get; set; }
    }
}
