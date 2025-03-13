using ProjectService_gRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Application.Common.Interfaces.Services.Queue
{
    public interface IQueueLogService
    {
        Task<bool> CreateLogAsync(CreateQueueLogRequest request, CancellationToken cancellationToken);
    }
}
