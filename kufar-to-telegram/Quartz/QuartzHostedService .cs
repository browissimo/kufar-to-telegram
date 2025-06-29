using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;

namespace TelegramTestProject
{
    public class QuartzHostedService : IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        private readonly JobSchedule _jobSchedule;
        private IScheduler _scheduler;

        public QuartzHostedService(ISchedulerFactory schedulerFactory, IJobFactory jobFactory, JobSchedule jobSchedule)
        {
            _schedulerFactory = schedulerFactory;
            _jobFactory = jobFactory;
            _jobSchedule = jobSchedule;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("▶️ QuartzHostedService запущен");
            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            _scheduler.JobFactory = _jobFactory;

            var job = JobBuilder.Create(_jobSchedule.JobType)
                .WithIdentity(_jobSchedule.JobType.FullName!)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{_jobSchedule.JobType.FullName}.trigger")
                .StartNow()
                .WithCronSchedule(_jobSchedule.CronExpression)
                .Build();

            await _scheduler.ScheduleJob(job, trigger, cancellationToken);
            await _scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _scheduler.Shutdown(cancellationToken);
        }
    }
}