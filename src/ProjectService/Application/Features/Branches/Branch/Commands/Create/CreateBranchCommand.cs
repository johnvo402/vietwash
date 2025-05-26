using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Application.Features.Common.Projections.Branches.Branch;
using Mediator;

namespace Application.Features.Branches.Branch.Commands.Create
{
    public class CreateBranchCommand : BranchModel, IRequest<CreateBranchResponse>
    {
    }
}
