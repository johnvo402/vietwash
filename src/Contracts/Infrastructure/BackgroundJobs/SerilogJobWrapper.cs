using Application.Jobs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.BackgroundJobs
{
    public class SerilogJobWrapper<T> where T : IJob
    {
        private readonly T _job;
        private readonly ILogger _logger;

        public SerilogJobWrapper(T job)
        {
            _job = job;
            _logger = Log.ForContext<T>();
        }

        public async Task ExecuteAsync()
        {
            var jobName = typeof(T).Name;

            try
            {
                await _job.ExecuteAsync();
                _logger.Information("✅ [JOB SUCCESS] Job {JobName} completed at {Time}", jobName, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ [JOB FAILED] Job {JobName} failed at {Time}", jobName, DateTime.UtcNow);
            }
        }
    }
}
