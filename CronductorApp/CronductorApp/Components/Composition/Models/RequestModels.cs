namespace CronductorApp.Components.Composition.Models;

// Temp(ish) models until back-end wireup

public class RequestModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastExecuted { get; set; }

    // Extended request configuration for add/edit
    public string Method { get; set; } = "GET";
    public string? ContentType { get; set; } = "application/json";
    public List<HeaderItem> Headers { get; set; } = new();
    public string? BodyJson { get; set; } = string.Empty;
}

public class HistoryModel
{
    public string Id { get; set; } = string.Empty;
    public string RequestName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long Duration { get; set; }
    public DateTime ExecutedAt { get; set; }
    public string Headers { get; set; } = string.Empty;
    public string ResponseHeaders { get; set; } = string.Empty;
    public string ResponseBody { get; set; } = string.Empty;
    public bool IsExpanded { get; set; }
}

public class LiveLogModel
{
    public string Id { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class HeaderItem
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
