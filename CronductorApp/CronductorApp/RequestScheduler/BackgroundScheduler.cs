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
        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        
        try
        {
            while (_scheduleService.TryPeekNextOccurrence(out var executionTime) && executionTime <= nowUtc)
            {
                var occurrence = _scheduleService.DequeueNextOccurrence();
                if (!_scheduleService.TryGetDefinition(occurrence.RequestId, out var definition))
                {
                    continue;
                }

                _logger.LogDebug("Processing scheduled request {RequestName} at {ExecutionTime}",
                    definition.Name, executionTime);

                _scheduleService.RequeueAfterExecution(definition);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (definition.Version != occurrence.Version || !definition.IsActive)
                        {
                            return;
                        }

                        await _requestProcessor.ProcessRequest(definition);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing request {Name}, will retry on next schedule",
                            definition.Name);
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