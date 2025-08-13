using Shared.Kernel.Common;

namespace Domain.Aggregates.Accounts
{
    public class AccountContact : BaseEntity<long>
    {
        public long AccountId { get; set; }
        public string? Address { get; set; }
        public string? Commune { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }
        public string? CommuneCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? ProvinceCode { get; set; }
        public string? Street { get; set; }

        public virtual Account Account { get; set; } = default!;
    }
}
