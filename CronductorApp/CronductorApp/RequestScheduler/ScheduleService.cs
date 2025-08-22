using CronductorApp.RequestScheduler.Models;
using Cronos;

namespace CronductorApp.RequestScheduler;

public class ScheduleService
{
    private readonly PriorityQueue<ScheduledRequest, DateTime> _scheduleQueue = new();
    private readonly object _queueLock = new();

    public bool AddSchedule(ScheduledRequest request)
    {
        // todo - validate these before adding to queue?
        lock (_queueLock)
        {
            var nextOccurrence = EvaluateNextOccurrence(request);
            if (nextOccurrence.HasValue)
            {
                _scheduleQueue.Enqueue(request, nextOccurrence.Value);
                return true;
            }
        }

        return false;
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

    private static DateTime? EvaluateNextOccurrence(ScheduledRequest request)
    {
        var cron = CronExpression.Parse(request.CronSchedule, CronFormat.IncludeSeconds);
        return cron.GetNextOccurrence(DateTime.UtcNow);
    }
}