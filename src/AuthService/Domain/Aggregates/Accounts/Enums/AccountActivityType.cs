using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Accounts.Enums
{
    public enum AccountActivityType : byte
    {
        Login = 1,
        Logout = 2,
        ResetPassword = 3,
        ChangePassword = 4
    }
}
