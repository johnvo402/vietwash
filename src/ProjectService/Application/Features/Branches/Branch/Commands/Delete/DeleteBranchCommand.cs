using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mediator;

namespace Application.Features.Branches.Branch.Commands.Delete
{
    public record class DeleteBranchCommand(long  branchId) : IRequest
    {

    }
}
