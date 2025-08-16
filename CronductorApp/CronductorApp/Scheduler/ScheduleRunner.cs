
namespace CronductorApp.Scheduler;

public class ScheduleRunner : BackgroundService
{
    private PriorityQueue<ScheduledRequest, DateTime> _scheduleQueue = new();

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        // todo - clear down PQ
        
        return Task.CompletedTask;
    }

    private static void ExecuteScheduledTasks(object? state)
    {
        if (state is not ScheduledRequest scheduledRequest)
        {
            Console.WriteLine("Invalid schedule state.");
            return;
        }
        
        Console.WriteLine($"Executed {scheduledRequest.Name} at {DateTime.Now}");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // dunno what to do with this?
        return Task.CompletedTask;
    }
}