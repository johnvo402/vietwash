using ErrorOr;
using Micro.Shared.Application.Security.Request;
using Micro.Shared.Infrastructure.CurrentUserProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Micro.Shared.Infrastructure.Security.PolicyEnforcer
{
    public interface IPolicyEnforcer
    {
        public ErrorOr<Success> Authorize(
            ICurrentUser currentUser,
            string policy);
    }
}
