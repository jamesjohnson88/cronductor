using CronductorApp.RequestScheduler.Models;
using Cronos;
using Microsoft.Extensions.Logging;

namespace CronductorApp.RequestScheduler;

public class ScheduleService
{
    private readonly PriorityQueue<ScheduledRequest, DateTime> _scheduleQueue = new();
    private readonly object _queueLock = new();
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(ILogger<ScheduleService> logger)
    {
        _logger = logger;
    }

    public bool AddSchedule(ScheduledRequest request)
    {
        try
        {
            var nextOccurrence = EvaluateNextOccurrence(request);
            if (!nextOccurrence.HasValue)
            {
                _logger.LogWarning("Could not determine next occurrence for request {RequestName}", request.Name);
                return false;
            }

            lock (_queueLock)
            {
                _scheduleQueue.Enqueue(request, nextOccurrence.Value);
                _logger.LogInformation("Added schedule for {RequestName} at {NextOccurrence}",
                    request.Name, nextOccurrence.Value);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add schedule for request {RequestName}: {Message}", request.Name, ex.Message);
            return false;
        }
    }

    public void RemoveSchedule(ScheduledRequest request)
    {
        throw new NotImplementedException();
    }

    public bool PeekNextSchedule(out DateTime nextOccurrence)
    {
        lock (_queueLock)
        {
            return _scheduleQueue.TryPeek(out _, out nextOccurrence);
        }
    }

    public ScheduledRequest DequeueNextSchedule()
    {
        lock (_queueLock)
        {
            return _scheduleQueue.Dequeue();
        }
    }

    private DateTime? EvaluateNextOccurrence(ScheduledRequest request)
    {
        try
        {
            var cron = CronExpression.Parse(request.CronSchedule, CronFormat.IncludeSeconds);
            var nextOccurrence = cron.GetNextOccurrence(DateTime.UtcNow);
            if (nextOccurrence.HasValue)
            {
                _logger.LogDebug("Next occurrence for {RequestName} with cron '{CronSchedule}': {NextOccurrence}",
                    request.Name, request.CronSchedule, nextOccurrence.Value);
            }

            return nextOccurrence;
        }
        catch (CronFormatException ex)
        {
            _logger.LogError(ex, "Invalid cron expression '{CronSchedule}' for request {RequestName}: {Message}",
                request.CronSchedule, request.Name, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error parsing cron expression '{CronSchedule}' for request {RequestName}: {Message}",
                request.CronSchedule, request.Name, ex.Message);
            return null;
        }
    }
}