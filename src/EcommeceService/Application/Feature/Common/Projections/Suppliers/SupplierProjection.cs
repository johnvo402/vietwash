using Domain.Aggregates.Suppliers.Enum;
using JohnChum.SharedKernel.Application.Common;


namespace Application.Feature.Common.Projections.Suppliers
{
	public class SupplierProjection : BaseResponse
	{
		public string Name { get; set; } = string.Empty;
		public string Code { get; set; } = string.Empty;
		public string? Email { get; set; }
        public string? Address { get; set; }
		public string? Phone { get; set; }
		public string? Description { get; set; }
		public SupplierStatus Status { get; set; }
	}
}
