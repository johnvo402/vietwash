using Contracts.Dtos.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Common.Projections.Reports
{
    public class ReportFilter : QueryParamRequest
    {
        public long From { get; set; }
        public long To { get; set; }
        public List<long>? BranchIds { get; set; }
        public string? SearchKeywords { get; set; }
    }
}