
namespace CronductorApp.RequestScheduler;

public class BackgroundScheduler(
    ScheduleService scheduleService,
    TimeProvider timeProvider,
    RequestProcessor requestProcessor) : BackgroundService
{   
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var oneSecondTicker = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (await oneSecondTicker.WaitForNextTickAsync(stoppingToken))
        {
            await TryProcessNextScheduledRequestAsync(stoppingToken);
        }
    }
    
    private async Task TryProcessNextScheduledRequestAsync(CancellationToken cancellationToken)
    {
        var hasNext = scheduleService.ScheduleQueue.TryPeek(out _, out var executionTime);
        if (hasNext && executionTime < timeProvider.GetLocalNow())
        {
            var nextScheduledRequest = scheduleService.ScheduleQueue.Dequeue();
            await requestProcessor.ProcessRequest(nextScheduledRequest, cancellationToken);
        }
    }
}