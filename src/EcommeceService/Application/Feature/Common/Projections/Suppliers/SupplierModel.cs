
using Domain.Aggregates.Enums;

namespace Application.Feature.Common.Projections.Suppliers
{
    public class SupplierModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Code { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Description { get; set; }
        public ActivationStatus? Status { get; set; }

    }
}
