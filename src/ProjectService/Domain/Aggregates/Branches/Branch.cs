using Ardalis.GuardClauses;
using Domain.Aggregates.Branches.Enums;
using Domain.Aggregates.Warehouses;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Branches
{
    public class Branch : AggregateRoot
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public bool Main { get; set; } = default!;
        public bool Disable { get; set; } = default!;
        public BranchStatus Status { get; set; } = default!;
        public string? Email { get; set; }
        public string? PhoneCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AddressName { get; set; }
        public string? CommuneName { get; set; }
        public string? CommuneCode { get; set; }
        public string? DistrictName { get; set; }
        public string? DistrictCode { get; set; }
        public string? ProvinceName { get; set; }
        public string? ProvinceCode { get; set; }
        public string? Street { get; set; }
        public string? Slug { get; set; }
        public ICollection<BranchUser> BranchUsers { get; set; } = [];
        public ICollection<BranchProduct> BranchProducts { get; set; } = [];
        public ICollection<Warehouse> Warehouses { get; set; } = [];

        public Branch(
            string name,
            string? code,
            bool main,
            BranchStatus status,
            string? email,
            string? phoneCode,
            string? phoneNumber,
            string? addressName,
            string? communeName,
            string? communeCode,
            string? districtName,
            string? districtCode,
            string? provinceName,
            string? provinceCode,
            string? street
        )
        {
            Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
            Code = Guard.Against.NullOrWhiteSpace(code, nameof(code));
            Status = Guard.Against.EnumOutOfRange(status, nameof(status));

            Main = main;
            Email = email;
            PhoneCode = phoneCode;
            PhoneNumber = phoneNumber;
            AddressName = addressName;
            CommuneName = communeName;
            CommuneCode = communeCode;
            DistrictName = districtName;
            DistrictCode = districtCode;
            ProvinceName = provinceName;
            ProvinceCode = provinceCode;
            Street = street;
        }

        public void Update(
            string? name,
            string? code,
            bool main,
            BranchStatus status,
            string? email,
            string? phoneCode,
            string? phoneNumber,
            string? addressName,
            string? communeName,
            string? communeCode,
            string? districtName,
            string? districtCode,
            string? provinceName,
            string? provinceCode,
            string? street
        )
        {
            if (!string.IsNullOrEmpty(name))
                Name = name;
            if (!string.IsNullOrEmpty(code))
                Code = code;

            Main = main;
            Status = status;

            if (!string.IsNullOrEmpty(email))
                Email = email;
            if (!string.IsNullOrEmpty(phoneCode))
                PhoneCode = phoneCode;
            if (!string.IsNullOrEmpty(phoneNumber))
                PhoneNumber = phoneNumber;
            if (!string.IsNullOrEmpty(addressName))
                AddressName = addressName;
            if (!string.IsNullOrEmpty(communeName))
                CommuneName = communeName;
            if (!string.IsNullOrEmpty(communeCode))
                CommuneCode = communeCode;
            if (!string.IsNullOrEmpty(districtName))
                DistrictName = districtName;
            if (!string.IsNullOrEmpty(districtCode))
                DistrictCode = districtCode;
            if (!string.IsNullOrEmpty(provinceName))
                ProvinceName = provinceName;
            if (!string.IsNullOrEmpty(provinceCode))
                ProvinceCode = provinceCode;
            if (!string.IsNullOrEmpty(street))
                Street = street;
        }

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
