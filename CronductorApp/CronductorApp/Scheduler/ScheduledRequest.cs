namespace CronductorApp.Scheduler;

public record ScheduledRequest(string Name, int FrequencySeconds)
{
    public string Name { get; set; } = Name;

    public int FrequencySeconds { get; set; } = FrequencySeconds;
}