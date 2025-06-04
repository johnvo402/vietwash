using Domain.Aggregates.Accounts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Otp
{

    public class CreatePinRequest
    {
        public long AccountId { get; set; }
        public string To { get; set; } = default!;
        public AccountActivityType Type { get; set; } = default!;

    }
}
