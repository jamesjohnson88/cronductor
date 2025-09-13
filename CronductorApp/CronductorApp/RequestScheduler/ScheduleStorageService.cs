using CronductorApp.RequestScheduler.Models;

namespace CronductorApp.RequestScheduler;

// todo - persist to a sqlite db or json file
public class ScheduleStorageService(ILogger<ScheduleStorageService> logger)
{
    private readonly List<ScheduledRequest> _scheduledRequests = [];
    
    public async Task<IList<ScheduledRequest>> GetScheduledRequests()
    {
        return _scheduledRequests;
    }
    
    public async Task StoreScheduledRequest(ScheduledRequest request)
    {
        _scheduledRequests.Add(request);
        await Task.CompletedTask;
    }

    public async Task UpdateScheduledRequest(ScheduledRequest request)
    {
        var existingRequest = _scheduledRequests.SingleOrDefault(r => r.Id == request.Id);
        if (existingRequest != null)
        {
            _scheduledRequests.Remove(existingRequest);
            _scheduledRequests.Add(request);
        }
        else
        {
            logger.LogWarning("Scheduled request with Id {RequestId} not found for update", request.Id);
        }
    }

    public async Task DeleteScheduledRequest(string requestId)
    {
        var existingRequest = _scheduledRequests.SingleOrDefault(r => r.Id == requestId);
        if (existingRequest != null)
        {
            _scheduledRequests.Remove(existingRequest);
        }
        else
        {
            logger.LogWarning("Scheduled request with Id {RequestId} not found for update", requestId);
        }
    }
}