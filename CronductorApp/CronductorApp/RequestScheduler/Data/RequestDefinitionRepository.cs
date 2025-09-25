using CronductorApp.RequestScheduler.Models;

namespace CronductorApp.RequestScheduler.Data;

// todo - persist to a sqlite db or json file
public class RequestDefinitionRepository(ILogger<RequestDefinitionRepository> logger)
{
    private readonly List<RequestDefinition> _scheduledRequests = [];

    public async Task<RequestDefinition?> GetByIdAsync(string requestId)
    {
        var existingRequest = _scheduledRequests.SingleOrDefault(r => r.Id == requestId);
        return await Task.FromResult(existingRequest);
    }

    public async Task<IList<RequestDefinition>> GetScheduledRequests()
    {
        return await Task.FromResult(_scheduledRequests);
    }

    public async Task AddOrUpdateDefinitionAsync(RequestDefinition requestDefinition)
    {
        var existingRequest = _scheduledRequests.SingleOrDefault(r => r.Id == requestDefinition.Id);
        if (existingRequest != null)
        {
            _scheduledRequests.Remove(existingRequest);
        }
        
        _scheduledRequests.Add(requestDefinition);
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