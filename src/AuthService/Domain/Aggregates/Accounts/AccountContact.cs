using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Accounts
{
    public class AccountContact : BaseEntity<long>
    {
        public long AccountId { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string Commune { get; set; } = default!;
        public string District { get; set; } = default!;
        public string Province { get; set; } = default!;
        public string CommuneCode { get; set; } = default!;
        public string DistrictCode { get; set; } = default!;
        public string ProvinceCode { get; set; } = default!;
        public string? Street { get; set; }

        public virtual Account Account { get; set; } = default!;
    }
}
