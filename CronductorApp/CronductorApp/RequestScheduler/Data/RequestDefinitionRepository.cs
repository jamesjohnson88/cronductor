using System.Data;
using CronductorApp.Data;
using CronductorApp.RequestScheduler.Models;
using Dapper;

namespace CronductorApp.RequestScheduler.Data;

public class RequestDefinitionRepository(
    ILogger<RequestDefinitionRepository> logger,
    IDbConnectionFactory dbConnectionFactory)
{
    [Obsolete(message:"Replace with Sqlite/Dapper implementation")]
    private readonly List<RequestDefinition> _scheduledRequests = [];

    // todo - on launch, the service should load existing definitions from the database
    
    public async Task<RequestDefinition?> GetByIdAsync(string requestId)
    {
        const string sql = "SELECT * FROM RequestDefinitions WHERE Id = @Id";
        var connection = dbConnectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<RequestDefinition>(sql, new { Id = requestId });
    }

    public async Task<IList<RequestDefinition>> GetScheduledRequests()
    {
        const string sql = "SELECT * FROM RequestDefinitions";
        var connection = dbConnectionFactory.CreateConnection();
        var requests = await connection.QueryAsync<RequestDefinition>(sql);
        return requests.ToList();
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