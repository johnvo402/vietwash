using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
