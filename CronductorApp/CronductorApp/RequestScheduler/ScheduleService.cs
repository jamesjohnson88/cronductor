using CronductorApp.RequestScheduler.Data;
using CronductorApp.RequestScheduler.Models;
using Cronos;

namespace CronductorApp.RequestScheduler;

public readonly record struct ScheduledOccurrence(string RequestId, int Version);

public class ScheduleService(
    ILogger<ScheduleService> logger,
    RequestDefinitionRepository repository)
{
    public List<RequestDefinition> RequestDefinitions => 
        _definitions.Select(kvp => kvp.Value)
            .OrderBy(r => r.Name)
            .ToList();
    
    private readonly object _queueLock = new();
    private readonly PriorityQueue<ScheduledOccurrence, DateTime> _scheduleQueue = new();
    private readonly Dictionary<string, RequestDefinition> _definitions = new();

    public async Task AddOrUpdateDefinitionAsync(RequestDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        try
        {
            var addedNew = _definitions.TryAdd(definition.Id, definition);
            if (!addedNew)
            {
                definition.Version++;
                _definitions[definition.Id] = definition;
            }
            
            await repository.AddOrUpdateDefinitionAsync(definition);

            if (definition.IsActive)
            {
                EnqueueNextOccurrence(definition);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add/update definition {RequestName}: {Message}", definition.Name, ex.Message);
            throw;
        }
    }

    public async Task DeleteDefinitionAsync(string requestId)
    {
        if (string.IsNullOrWhiteSpace(requestId)) return;
        try
        {
            _definitions.Remove(requestId);
            await repository.DeleteScheduledRequest(requestId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete definition {RequestId}: {Message}", requestId, ex.Message);
            throw;
        }
    }

    public async Task PauseDefinitionAsync(string requestId)
    {
        if (!_definitions.TryGetValue(requestId, out var definition))
        {
            return;
        }
        
        definition.IsActive = false;
        await repository.AddOrUpdateDefinitionAsync(definition);
        _definitions[requestId] = definition;
    }

    public async Task ResumeDefinitionAsync(string requestId)
    {
        if (!_definitions.TryGetValue(requestId, out var definition))
        {
            return;
        }
        
        definition.IsActive = true;
        await repository.AddOrUpdateDefinitionAsync(definition);
        _definitions[requestId] = definition;
        EnqueueNextOccurrence(definition);
    }

    public bool TryPeekNextOccurrence(out DateTime nextOccurrenceUtc)
    {
        lock (_queueLock)
        {
            while (_scheduleQueue.TryPeek(out var occurrence, out var executeAtUtc))
            {
                if (IsOccurrenceValid(occurrence))
                {
                    nextOccurrenceUtc = executeAtUtc;
                    return true;
                }
                
                // drop stale occurrence
                _scheduleQueue.Dequeue();
                logger.LogDebug("TryPeek() Dropped stale occurrence for request {RequestId} version {Version}",
                    occurrence.RequestId, occurrence.Version);
            }
        }

        nextOccurrenceUtc = default;
        return false;
    }

    public ScheduledOccurrence DequeueNextOccurrence()
    {
        lock (_queueLock)
        {
            while (_scheduleQueue.TryDequeue(out var occurrence, out _))
            {
                if (IsOccurrenceValid(occurrence))
                {
                    return occurrence;
                }
                
                // drop stale occurrence
                logger.LogDebug("Dequeue() Dropped stale occurrence for request {RequestId} version {Version}",
                    occurrence.RequestId, occurrence.Version);
            }
        }

        throw new InvalidOperationException("No scheduled occurrences are available.");
    }

    public bool TryGetDefinition(string requestId, out RequestDefinition definition)
    {
        return _definitions.TryGetValue(requestId, out definition!);
    }

    public void RequeueAfterExecution(RequestDefinition definition)
    {
        if (!definition.IsActive) return;
        EnqueueNextOccurrence(definition);
    }

    private void EnqueueNextOccurrence(RequestDefinition definition)
    {
        var next = EvaluateNextOccurrence(definition);
        if (!next.HasValue) return;
        lock (_queueLock)
        {
            _scheduleQueue.Enqueue(new ScheduledOccurrence(definition.Id, definition.Version), next.Value);
            logger.LogInformation("Queued next occurrence for {RequestName} at {ExecuteAt}", definition.Name, next.Value);
        }
    }

    private bool IsOccurrenceValid(ScheduledOccurrence occurrence)
    {
        logger.LogInformation("All requests: {Requests}", string.Join(", ", _definitions.Keys));
        if (!_definitions.TryGetValue(occurrence.RequestId, out var def))
        {
            logger.LogWarning("No definition found for request {RequestName}", occurrence.RequestId);
            return false;
        }

        if (!def.IsActive)
        {
            logger.LogDebug("Definition for {RequestId} is inactive, skipping occurrence", occurrence.RequestId);
            return false;
        }
        
        logger.LogDebug("Occurrence for {RequestId} is at version {OccurrenceVersion}, current definition version is {DefinitionVersion}",
            occurrence.RequestId, occurrence.Version, def.Version);
        
        return def.Version == occurrence.Version;
    }

    private DateTime? EvaluateNextOccurrence(RequestDefinition requestDefinition)
    {
        try
        {
            var cron = CronExpression.Parse(requestDefinition.CronSchedule, CronFormat.IncludeSeconds);
            var nextOccurrence = cron.GetNextOccurrence(DateTime.UtcNow);
            if (nextOccurrence.HasValue)
            {
                logger.LogDebug("Next occurrence for {RequestName} with cron '{CronSchedule}': {NextOccurrence}",
                    requestDefinition.Name, requestDefinition.CronSchedule, nextOccurrence.Value);
            }

            return nextOccurrence;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing cron expression '{CronSchedule}' for request {RequestName}: {Message}",
                requestDefinition.CronSchedule, requestDefinition.Name, ex.Message);
            return null;
        }
    }
}