namespace CronductorApp.Scheduler;

public class ScheduleService // todo - pull out into interface IScheduleService
{
    // This class will manage the scheduling of requests and be responsible for interaction with the ScheduleRunner.

    public void AddSchedule(ScheduledRequest request)
    {
        // todo - calculate next executio then add to PQ
    }
    
    public DateTime EvaluateNextExecution(ScheduledRequest request)
    {
        // todo - evaluate next execution time based on request's cron expression
        return DateTime.Now.AddSeconds(10); // Placeholder
    }
}