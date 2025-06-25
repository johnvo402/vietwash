using Application.Jobs;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.BackgroundJobs;

namespace Infrastructure.Services.BackgroundJobs
{
    public class JobScheduler
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IServiceProvider _serviceProvider;

        public JobScheduler(IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider)
        {
            _recurringJobManager = recurringJobManager;
            _serviceProvider = serviceProvider;
        }

        public void ScheduleJobs()
        {
            var jobs = new Dictionary<string, (Type JobType, string CronSchedule)>
            {
                { "check-customer-loyal", (typeof(CheckCustomerLoyal), "5 * * * *") }
            };

            foreach (var job in jobs)
            {
                var jobId = job.Key;
                var jobType = job.Value.JobType;
                var cronSchedule = job.Value.CronSchedule;

                _recurringJobManager.AddOrUpdate(
                    jobId,
                    () => RunJob(jobType),
                    cronSchedule
                );
            }
        }

        public async Task RunJob(Type jobType)
        {
            using var scope = _serviceProvider.CreateScope();
            var jobInstance = (IJob)scope.ServiceProvider.GetRequiredService(jobType);
            var wrapperType = typeof(SerilogJobWrapper<>).MakeGenericType(jobType);
            var wrapperInstance = (dynamic)scope.ServiceProvider.GetRequiredService(wrapperType);
            await wrapperInstance.ExecuteAsync();
        }
    }
}
