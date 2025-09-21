using CronductorApp.RequestScheduler.Models;

namespace CronductorApp.RequestScheduler.Data;

// todo - persist to a sqlite db or json file
public class RequestDefinitionRepository(ILogger<RequestDefinitionRepository> logger)
{
    private readonly List<RequestDefinitions> _scheduledRequests = [];

    public async Task<RequestDefinitions?> GetByIdAsync(string requestId)
    {
        var existingRequest = _scheduledRequests.SingleOrDefault(r => r.Id == requestId);
        return await Task.FromResult(existingRequest);
    }

    public async Task<IList<RequestDefinitions>> GetScheduledRequests()
    {
        return await Task.FromResult(_scheduledRequests);
    }

    public async Task AddOrUpdateDefinitionAsync(RequestDefinitions requestDefinitions)
    {
        var existingRequest = _scheduledRequests.SingleOrDefault(r => r.Id == requestDefinitions.Id);
        if (existingRequest != null)
        {
            _scheduledRequests.Remove(existingRequest);
        }
        
        _scheduledRequests.Add(requestDefinitions);
        await Task.CompletedTask;
    }

    [Obsolete(message: "Use AddOrUpdateDefinitionAsync instead")]
    public async Task UpdateScheduledRequest(RequestDefinitions requestDefinitions)
    {
        var existingRequest = _scheduledRequests.SingleOrDefault(r => r.Id == requestDefinitions.Id);
        if (existingRequest != null)
        {
            _scheduledRequests.Remove(existingRequest);
            _scheduledRequests.Add(requestDefinitions);
        }
        else
        {
            logger.LogWarning("Scheduled request with Id {RequestId} not found for update", requestDefinitions.Id);
        }

        await Task.CompletedTask;
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

        await Task.CompletedTask;
    }
}