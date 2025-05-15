using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Services
{
    public class Unit : BaseEntity
    {
        public string Name { get; set; } = default!;
		public string Path { get; set; } = default!;	
		//public ICollection<ProductSupplying> ProductSupplyings { get; set; } = [];

	}
}
