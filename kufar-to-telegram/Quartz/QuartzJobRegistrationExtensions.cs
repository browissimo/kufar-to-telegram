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
            services.AddSingleton(new JobSchedule(
                jobType: typeof(KyfarCheckerJob),
                cronExpression: "0 */10 * * * ?"
            ));
        }
    }
}