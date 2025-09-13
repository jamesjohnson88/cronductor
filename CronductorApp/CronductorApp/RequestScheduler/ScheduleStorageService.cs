using CronductorApp.RequestScheduler.Models;

namespace CronductorApp.RequestScheduler;

// todo - persist to a sqlite db or json file
public class ScheduleStorageService(ILogger<ScheduleStorageService> logger)
{
    public async Task GetScheduledRequests()
    {
        await Task.CompletedTask;
    }
    
    public async Task StoreScheduledRequest(ScheduledRequest request)
    {
        await Task.CompletedTask;
    }
    
    public async Task DeleteScheduledRequest(string requestId)
    {
        await Task.CompletedTask;
    }
}