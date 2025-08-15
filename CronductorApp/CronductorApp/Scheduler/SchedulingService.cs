
using System.Collections.Concurrent;

namespace CronductorApp.Scheduler;

public class SchedulingService : BackgroundService
{
    private ConcurrentBag<Timer> timers = new();

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        List<TestSchedule> schedules = new()
        {
            new TestSchedule("Schedule One", 10),
            new TestSchedule("Schedule Two", 15),
            new TestSchedule("Schedule Three", 30)
        };

        foreach (var schedule in schedules)
        {
            timers.Add(new Timer(
                ExecuteScheduledTasks,
                schedule,
                TimeSpan.Zero, 
                TimeSpan.FromSeconds(schedule.FrequencySeconds)));
        }
        
        return base.StartAsync(cancellationToken);
    }

    private static void ExecuteScheduledTasks(object? state)
    {
        if (state is not TestSchedule schedule)
        {
            Console.WriteLine("Invalid schedule state.");
            return;
        }
        
        Console.WriteLine($"Executed {schedule.Name} at {DateTime.Now}");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}

public class TestSchedule(string name, int frequencySeconds)
{
    public string Name { get; set; } = name;

    public int FrequencySeconds { get; set; } = frequencySeconds;
}