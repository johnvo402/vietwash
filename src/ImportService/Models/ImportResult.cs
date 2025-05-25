using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImportService.Models
{
    public class ImportResult
    {
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
    }
}
