using Quartz;
using Quartz.Spi;

public class SingletonJobFactory : IJobFactory
{
    private readonly IServiceProvider _serviceProvider;

    public SingletonJobFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var job = _serviceProvider.GetService(bundle.JobDetail.JobType);
        if (job == null)
            throw new InvalidOperationException($"Не удалось создать экземпляр задания: {bundle.JobDetail.JobType.FullName}");


        Console.WriteLine($"Удалось создать экземпляр задания: {bundle.JobDetail.JobType.FullName}");
        return (IJob)job;
    }

    public void ReturnJob(IJob job)
    {
        // Необходимо реализовать этот метод, если требуется какая-то логика возврата задания.
    }
}