namespace Contracts.Infrastructure.Common
{
    public class OrgSetting
    {
        public int VatPercent { get; set; } = default!;
        public string OrgName { get; set; } = default!;
        public string OrgTaxCode { get; set; } = default!;
        public string OrgAddress { get; set; } = default!;
        public string OrgPhone { get; set; } = default!;
        public string Logo { get; set; } = default!;
        public string Stamp { get; set; } = default!;
    }
}
