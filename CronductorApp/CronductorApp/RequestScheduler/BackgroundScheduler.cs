namespace CronductorApp.RequestScheduler;

public class BackgroundScheduler : BackgroundService
{
    private readonly ScheduleService _scheduleService;
    private readonly TimeProvider _timeProvider;
    private readonly RequestProcessor _requestProcessor;
    private readonly ILogger<BackgroundScheduler> _logger;

    public BackgroundScheduler(
        ScheduleService scheduleService,
        TimeProvider timeProvider,
        RequestProcessor requestProcessor,
        ILogger<BackgroundScheduler> logger)
    {
        _scheduleService = scheduleService;
        _timeProvider = timeProvider;
        _requestProcessor = requestProcessor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background scheduler started");

        try
        {
            using var oneSecondTicker = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await oneSecondTicker.WaitForNextTickAsync(stoppingToken))
            {
                TryProcessNextScheduledRequestAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Background scheduler stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background scheduler encountered an error");
            throw;
        }
    }

    private void TryProcessNextScheduledRequestAsync(CancellationToken cancellationToken)
    {
        try
        {
            var hasNext = _scheduleService.PeekNextSchedule(out var executionTime);
            if (hasNext && executionTime < _timeProvider.GetLocalNow())
            {
                var nextScheduledRequest = _scheduleService.DequeueNextSchedule();
                _logger.LogDebug("Processing scheduled request {RequestName} at {ExecutionTime}",
                    nextScheduledRequest.Name, executionTime);

                // Re-add so that schedules with close executions are not blocked by current execution(s)
                // We're taking this execution, then when re-added, the scheduler will evaluate and queue the next one
                _scheduleService.AddSchedule(nextScheduledRequest);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _requestProcessor.ProcessRequest(nextScheduledRequest);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing request {RequestName}, will retry on next schedule",
                            nextScheduledRequest.Name);
                    }
                }, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing next scheduled request");
        }
    }
}