using System.Net.Mime;

namespace CronductorApp.RequestScheduler.Models;

public class ScheduledRequest
{
    public string Name { get; set; } = string.Empty;
    
    public string Method { get; set; } = string.Empty;
    
    public string Url { get; set; } = string.Empty;
    
    public string? ContentType { get; set; } = MediaTypeNames.Application.Json;
    
    public Dictionary<string, string> Headers { get; set; } = new();
    
    public object? Body { get; set; } = JsonContent.Create(new { });
    
    public string CronSchedule { get; set; } = string.Empty;

    // Other config/options can come later, such as jitter, burst, etc.
}