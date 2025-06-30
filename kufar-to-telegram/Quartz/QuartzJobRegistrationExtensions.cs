using Microsoft.Extensions.DependencyInjection;
using Quartz;
using TelegramTestProject.Jobs;

namespace MPBot.Services.Quartz
{
    public static class QuartzJobRegistrationExtensions
    {
        public static void QuartzJobRegistration(this IServiceCollection services)
        {
            services.AddSingleton<KyfarCheckerJob>();
            //services.AddSingleton<IJob, KyfarCheckerJob>();
            services.AddSingleton(new JobSchedule(
                jobType: typeof(KyfarCheckerJob),
                cronExpression: "15 * * * * ?"
                //cronExpression: "0 15 * * * ?"
                //cronExpression: "1 * * * * ?"
            ));
        }
    }
}