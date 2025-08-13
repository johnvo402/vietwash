using Domain.Aggregates.Accounts;

namespace Application.Features.Common.Projections.Accounts
{
    public class AccountContactProjection
    {
        public string? Address { get; set; }
        public string? Commune { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }
        public string? CommuneCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? ProvinceCode { get; set; }
        public string? Street { get; set; }

        public virtual void MappingFrom(AccountContact contact)
        {
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
