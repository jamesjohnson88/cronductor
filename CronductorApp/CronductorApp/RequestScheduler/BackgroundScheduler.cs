
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
            TryProcessNextScheduledRequestAsync(stoppingToken);
        }
    }
    
    private void TryProcessNextScheduledRequestAsync(CancellationToken cancellationToken)
    {
        var hasNext = scheduleService.PeekNextSchedule(out var executionTime);
        if (hasNext && executionTime < timeProvider.GetLocalNow())
        {
            var nextScheduledRequest = scheduleService.DequeueNextSchedule();
            _ = Task.Run(() => requestProcessor.ProcessRequest(nextScheduledRequest), cancellationToken);
            scheduleService.AddSchedule(nextScheduledRequest);
        }
    }
}