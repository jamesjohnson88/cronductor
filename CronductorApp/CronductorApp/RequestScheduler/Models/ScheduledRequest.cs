namespace CronductorApp.RequestScheduler.Models;

public record ScheduledRequest(string Name, int FrequencySeconds)
{
    public string Name { get; set; } = Name;

    public int FrequencySeconds { get; set; } = FrequencySeconds;
    
    // method, url, etc.
}