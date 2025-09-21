using System.Net.Mime;
using CronductorApp.Components.Composition.Models;

namespace CronductorApp.RequestScheduler.Models;

public class RequestDefinitions
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public int Version { get; set; } = 1;

    public string Name { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? ContentType { get; set; } = MediaTypeNames.Application.Json;

    public List<HeaderItem> Headers { get; set; } = [];

    public object? Body { get; set; } = JsonContent.Create(new { });

    public string CronSchedule { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime LastExecuted { get; set; } = DateTime.MinValue;
}