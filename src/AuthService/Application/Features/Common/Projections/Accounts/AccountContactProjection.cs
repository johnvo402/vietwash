using Domain.Aggregates.Accounts;

namespace Application.Features.Common.Projections.Accounts
{
    public class AccountContactProjection
    {
        public string PhoneNumber { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string Commune { get; set; } = default!;
        public string District { get; set; } = default!;
        public string Province { get; set; } = default!;
        public string CommuneCode { get; set; } = default!;
        public string DistrictCode { get; set; } = default!;
        public string ProvinceCode { get; set; } = default!;
        public string? Street { get; set; }

        public virtual void MappingFrom(AccountContact contact)
        {
            PhoneNumber = contact.PhoneNumber;
            Address = contact.Address;
            Commune = contact.Commune;
            District = contact.District;
            Province = contact.Province;
            CommuneCode = contact.CommuneCode;
            DistrictCode = contact.DistrictCode;
            ProvinceCode = contact.ProvinceCode;
            Street = contact.Street;
        }
    }
}
