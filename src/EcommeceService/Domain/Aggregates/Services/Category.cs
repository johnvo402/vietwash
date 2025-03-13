using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Services
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = default!;
        public ICollection<Service> Services { get; set; } = [];
        
    }
}
