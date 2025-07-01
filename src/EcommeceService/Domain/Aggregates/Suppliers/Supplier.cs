using Domain.Aggregates.Inventories;
using Domain.Aggregates.Suppliers.Enum;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Suppliers
{
    public class Supplier : AggregateRoot
    {
        // Required properties - private set để đảm bảo bất biến sau khi khởi tạo
        public string Name { get; private set; } = default!;
        public string Code { get; set; }
        public SupplierStatus Status { get; private set; } = default!;

        // Optional or mutable properties
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Disable { get; set; } = false;

        public ICollection<ProductSupplying> ProductSupplyings { get; set; } = [];
        public ICollection<EquipmentSupplying> EquipmentSupplyings { get; set; } = [];

        // Constructor khởi tạo đầy đủ các trường bắt buộc
        public Supplier(string name, string code, SupplierStatus status)
        {
            Name = name.Trim();
            Code = code.Trim();
            Status = status;
        }

        // Factory method nếu bạn muốn kiểm soát logic tạo
        public Supplier(
            string name,
            string code,
            SupplierStatus status,
            string? email = null,
            string? address = null,
            string? phone = null,
            string? description = null
        )
        {
            Name = name?.Trim() ?? string.Empty;
            Code = code?.Trim() ?? string.Empty;
            Status = status;
            Email = email?.Trim() ?? string.Empty;
            Address = address?.Trim() ?? string.Empty;
            Phone = phone?.Trim() ?? string.Empty;
            Description = description?.Trim() ?? string.Empty;
        }

        public void Update(
            string? name = null,
            string? email = null,
            string? address = null,
            string? phone = null,
            string? description = null,
            SupplierStatus? status = null,
            bool? disable = null
        )
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name.Trim();
            if (!string.IsNullOrWhiteSpace(email))
                Email = email.Trim();
            if (!string.IsNullOrWhiteSpace(address))
                Address = address.Trim();
            if (!string.IsNullOrWhiteSpace(phone))
                Phone = phone.Trim();
            if (!string.IsNullOrWhiteSpace(description))
                Description = description.Trim();
            if (status.HasValue)
                Status = status.Value;
            if (disable.HasValue)
                Disable = disable.Value;
        }

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
