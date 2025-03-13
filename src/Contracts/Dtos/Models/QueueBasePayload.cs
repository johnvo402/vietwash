
namespace Contracts.Dtos.Models
{
    public class QueueBasePayload<T>
    {
        public Guid PayloadId { get; set; }

        public T? Payload { get; set; }
    }
}
