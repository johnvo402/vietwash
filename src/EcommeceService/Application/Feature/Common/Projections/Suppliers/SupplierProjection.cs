using Contracts.Application.Common;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Suppliers;

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
        public ActivationStatus Status { get; set; }

        public virtual void MappingFrom(Supplier supplier)
        {
            Id = supplier.Id;
            PublicId = supplier.PublicId;
            CreatedAt = supplier.CreatedAt;
            CreatedBy = supplier.CreatedBy;
            UpdatedAt = supplier.UpdatedAt;
            UpdatedBy = supplier.UpdatedBy;

            Name = supplier.Name;
            Code = supplier.Code;
            Email = supplier.Email;
            Address = supplier.Address;
            Phone = supplier.Phone;
            Description = supplier.Description;
            Status = supplier.Status;
        }
    }
}
