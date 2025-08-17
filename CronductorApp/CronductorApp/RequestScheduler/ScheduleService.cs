using CronductorApp.RequestScheduler.Models;

namespace CronductorApp.RequestScheduler;

public class ScheduleService
{
    public readonly PriorityQueue<ScheduledRequest, DateTime> ScheduleQueue = new();

    public void AddSchedule(ScheduledRequest request)
    {
        ScheduleQueue.Enqueue(request, EvaluateNextExecution(request));
    }
    
    private static DateTime EvaluateNextExecution(ScheduledRequest request)
    {
        // todo - evaluate next execution time based on request's cron expression
        return DateTime.Now.AddSeconds(10); // Placeholder
    }
}