using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using TelegramTestProject;

namespace MPBot.Services.Quartz
{
    public static class QuartzRegistrationExtensions
    {
        public static void QuartzRegistration(this IServiceCollection services)
        {
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddHostedService<QuartzHostedService>();
        }
    }
}