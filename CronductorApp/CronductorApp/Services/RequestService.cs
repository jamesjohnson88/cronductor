using CronductorApp.Components.Composition.Models;

namespace CronductorApp.Services;

public class RequestService
{
    public List<RequestModel> ScheduledRequests { get; set; } = new();
    public List<HistoryModel> RequestHistory { get; set; } = new();
    public List<LiveLogModel> LiveLogs { get; set; } = new();

    public event Action? OnScheduledRequestsChanged;

    public void InitializeSampleData()
    {
        // Sample scheduled requests
        ScheduledRequests = new List<RequestModel>
        {
            new()
            {
                Id = "1",
                Name = "Health Check API",
                Url = "https://api.example.com/health",
                Schedule = "Every 5 minutes",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-7),
                LastExecuted = DateTime.Now.AddMinutes(-2)
            },
            new()
            {
                Id = "2",
                Name = "Data Sync Job",
                Url = "https://api.example.com/sync",
                Schedule = "Daily at 2:00 AM",
                IsActive = false,
                CreatedAt = DateTime.Now.AddDays(-14),
                LastExecuted = DateTime.Now.AddDays(-1)
            }
        };

        // Sample request history
        RequestHistory = new List<HistoryModel>
        {
            new()
            {
                Id = "h1",
                RequestName = "Health Check API",
                Url = "https://api.example.com/health",
                Method = "GET",
                Status = "Success",
                StatusCode = 200,
                Duration = 150,
                ExecutedAt = DateTime.Now.AddMinutes(-5),
                Headers = "{\n \"User-Agent\": \"Cronductor/1.0\",\n \"Accept\": \"application/json\"\n}",
                ResponseHeaders = "{\n \"Content-Type\": \"application/json\",\n \"Cache-Control\": \"no-cache\"\n}",
                ResponseBody = "{\n \"status\": \"healthy\",\n \"timestamp\": \"2024-01-15T10:30:00Z\"\n}"
            },
            new()
            {
                Id = "h2",
                RequestName = "Data Sync Job",
                Url = "https://api.example.com/sync",
                Method = "POST",
                Status = "Error",
                StatusCode = 500,
                Duration = 5000,
                ExecutedAt = DateTime.Now.AddHours(-2),
                Headers = "{\n \"User-Agent\": \"Cronductor/1.0\",\n \"Content-Type\": \"application/json\"\n}",
                ResponseHeaders = "{\n \"Content-Type\": \"application/json\"\n}",
                ResponseBody = "{\n \"error\": \"Internal server error\",\n \"message\": \"Database connection failed\"\n}"
            }
        };

        // Sample live logs
        LiveLogs = new List<LiveLogModel>
        {
            new()
            {
                Id = "l1",
                Level = "info",
                Message = "Starting request execution",
                Details = "Request ID: req_12345\nURL: https://api.example.com/health",
                Timestamp = DateTime.Now.AddSeconds(-30)
            },
            new()
            {
                Id = "l2",
                Level = "success",
                Message = "Request completed successfully",
                Details = "Status: 200\nDuration: 150ms",
                Timestamp = DateTime.Now.AddSeconds(-25)
            },
            new()
            {
                Id = "l3",
                Level = "warning",
                Message = "Request took longer than expected",
                Details = "Expected: <1000ms\nActual: 1500ms",
                Timestamp = DateTime.Now.AddSeconds(-20)
            }
        };
    }

    public async Task AddRequest(RequestModel request)
    {
        ScheduledRequests.Add(request);
        OnScheduledRequestsChanged?.Invoke();
        await Task.CompletedTask;
    }

    public async Task EditRequest(string requestId, RequestModel updatedRequest)
    {
        var existingRequest = ScheduledRequests.FirstOrDefault(r => r.Id == requestId);
        if (existingRequest != null)
        {
            var index = ScheduledRequests.IndexOf(existingRequest);
            ScheduledRequests[index] = updatedRequest;
            OnScheduledRequestsChanged?.Invoke();
        }
        await Task.CompletedTask;
    }

    public async Task DeleteRequest(string requestId)
    {
        ScheduledRequests.RemoveAll(r => r.Id == requestId);
        OnScheduledRequestsChanged?.Invoke();
        await Task.CompletedTask;
    }
}
