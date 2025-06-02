using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Branches.Enums;
using JohnChum.SharedKernel.Domain.Common;

namespace Application.Features.Common.Projections.Branches.Branch
{
    public class BranchProjection : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public bool Main { get; set; } = default!;
        public BranchStatus Status { get; set; }
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
    }
}
