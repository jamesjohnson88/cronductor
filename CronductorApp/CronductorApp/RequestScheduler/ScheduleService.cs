using CronductorApp.RequestScheduler.Models;
using Cronos;

namespace CronductorApp.RequestScheduler;

public class ScheduleService(
    ILogger<ScheduleService> logger,
    ScheduleStorageService storageService)
{
    public List<ScheduledRequest> RequestDefinitions => GetScheduledRequests();
    
    private readonly PriorityQueue<ScheduledRequest, DateTime> _scheduleQueue = new();
    private readonly object _queueLock = new();

    // todo - this is not  a pure add - we need one for actual new schedules
    // and one for re-adding after execution
    public bool AddSchedule(ScheduledRequest request)
    {
        try
        {
            // todo - validate and return errors
            // will require a non-bool return...

            var nextOccurrence = EvaluateNextOccurrence(request);
            if (!nextOccurrence.HasValue)
            {
                logger.LogWarning("Could not determine next occurrence for request {RequestName}", request.Name);
                return false;
            }

            lock (_queueLock)
            {
                _scheduleQueue.Enqueue(request, nextOccurrence.Value);
                logger.LogInformation("Added schedule for {RequestName} at {NextOccurrence}",
                    request.Name, nextOccurrence.Value);
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add schedule for request {RequestName}: {Message}", request.Name, ex.Message);
            return false;
        }
    }

    public async Task AddScheduleAsync(ScheduledRequest request)
    {
        try
        {
            // todo - validate and return errors
            // will require a non-bool return...

            var nextOccurrence = EvaluateNextOccurrence(request);
            if (!nextOccurrence.HasValue)
            {
                logger.LogWarning(
                    "Could not determine next occurrence for request {RequestName}", 
                    request.Name);
                return;
            }
            
            await storageService.StoreScheduledRequest(request);
            
            lock (_queueLock)
            {
                _scheduleQueue.Enqueue(request, nextOccurrence.Value);
                logger.LogInformation(
                    "Added schedule for {RequestName} at {NextOccurrence}",
                    request.Name, 
                    nextOccurrence.Value);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex, 
                "Failed to add schedule for request {RequestName}: {Message}", 
                request.Name, 
                ex.Message);
        }
    }
    
    public async Task RemoveScheduleAsync(string requestId)
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
                logger.LogDebug("Next occurrence for {RequestName} with cron '{CronSchedule}': {NextOccurrence}",
                    request.Name, request.CronSchedule, nextOccurrence.Value);
            }

            return nextOccurrence;
        }
        catch (CronFormatException ex)
        {
            logger.LogError(ex, "Invalid cron expression '{CronSchedule}' for request {RequestName}: {Message}",
                request.CronSchedule, request.Name, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error parsing cron expression '{CronSchedule}' for request {RequestName}: {Message}",
                request.CronSchedule, request.Name, ex.Message);
            return null;
        }
    }
    
    private List<ScheduledRequest> GetScheduledRequests()
    {
        lock (_queueLock)
        {
            return _scheduleQueue.UnorderedItems
                .Select(item => item.Element)
                .OrderBy(rq => rq.Name)
                .ToList();
        }
    }
}