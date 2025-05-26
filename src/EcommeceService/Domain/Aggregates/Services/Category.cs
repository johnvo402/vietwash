using Domain.Aggregates.Services.Enums;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Services
{
    public class Category : BaseEntity<long>
	{
        public string Name { get; set; } = default!;
		public string Path { get; set; } = default!;
		public ActivationStatus status { get; set; } = default!;
        public ICollection<Service> Services { get; set; } = [];
    }
}
