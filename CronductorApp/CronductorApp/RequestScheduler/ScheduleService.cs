using CronductorApp.RequestScheduler.Models;
using Cronos;

namespace CronductorApp.RequestScheduler;

public class ScheduleService
{
    public readonly PriorityQueue<ScheduledRequest, DateTime> ScheduleQueue = new();

    public void AddSchedule(ScheduledRequest request)
    {
        // todo - validate these before adding to queue?
        var nextOccurrence = EvaluateNextOccurrence(request);
        if (nextOccurrence.HasValue)
        {
            ScheduleQueue.Enqueue(request, nextOccurrence.Value);
        }
        else
        {
            // temporary placeholder for error handling
            throw new Exception("Failed to add schedule!");
        }
    }

    public void RemoveSchedule(ScheduledRequest request)
    {
        throw new NotImplementedException();
    }
    
    private static DateTime? EvaluateNextOccurrence(ScheduledRequest request)
    {
        var cron = CronExpression.Parse(request.CronSchedule, CronFormat.IncludeSeconds);
        return cron.GetNextOccurrence(DateTime.UtcNow);
    }
}