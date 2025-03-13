using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Dtos.Models
{
    public class TokenBinding
    {
        public string Token { get; set; }
        public string Signature { get; set; }
        public string Nonce { get; set; }
        public long Timestamp { get; set; }
    }

}
