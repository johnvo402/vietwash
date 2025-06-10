using Domain.Aggregates.Enums;
using Domain.Aggregates.Suppliers.Enum;
using System.ComponentModel.DataAnnotations;

namespace Application.Feature.Common.Projections.Suppliers
{
    public class SupplierUpdateModel
    {
        public SupplierStatus? Status { get; set; }
        [Required]
        public SupplierModel Supplier { get; set; } = default!;
    }
}
