using CronductorApp.RequestScheduler.Data;
using CronductorApp.RequestScheduler.Models;
using Cronos;

namespace CronductorApp.RequestScheduler;

public readonly record struct ScheduledOccurrence(string RequestId, int Version, DateTime ExecuteAtUtc);

public class ScheduleService(
    ILogger<ScheduleService> logger,
    RequestDefinitionRepository repository)
{
    public List<RequestDefinitions> RequestDefinitions => 
        _definitions.Select(kvp => kvp.Value)
            .OrderBy(r => r.Name)
            .ToList();
    
    private readonly PriorityQueue<ScheduledOccurrence, DateTime> _scheduleQueue = new();
    private readonly object _queueLock = new();
    private readonly Dictionary<string, RequestDefinitions> _definitions = new();

    public async Task AddOrUpdateDefinitionAsync(RequestDefinitions definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        try
        {
            var addedNew = _definitions.TryAdd(definition.Name, definition);
            if (!addedNew)
            {
                definition.Version++;
                _definitions[definition.Name] = definition;
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
        // todo - remove any queued occurrences for this definition
    }

    public async Task ResumeDefinitionAsync(string requestId)
    {
        if (!_definitions.TryGetValue(requestId, out var definition))
        {
            return;
        }
        
        definition.IsActive = true;
        await repository.AddOrUpdateDefinitionAsync(definition);
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
            }
        }

        throw new InvalidOperationException("No scheduled occurrences are available.");
    }

    public bool TryGetDefinition(string requestId, out RequestDefinitions definition)
    {
        return _definitions.TryGetValue(requestId, out definition!);
    }

    public void RequeueAfterExecution(RequestDefinitions definition)
    {
        if (!definition.IsActive) return;
        EnqueueNextOccurrence(definition);
    }

    private void EnqueueNextOccurrence(RequestDefinitions definition)
    {
        var next = EvaluateNextOccurrence(definition);
        if (!next.HasValue) return;
        lock (_queueLock)
        {
            _scheduleQueue.Enqueue(new ScheduledOccurrence(definition.Id, definition.Version, next.Value), next.Value);
            logger.LogInformation("Queued next occurrence for {RequestName} at {ExecuteAt}", definition.Name, next.Value);
        }
    }

    private bool IsOccurrenceValid(ScheduledOccurrence occurrence)
    {
        if (!_definitions.TryGetValue(occurrence.RequestId, out var def))
        {
            return false;
        }

        if (!def.IsActive)
        {
            return false;
        }
        
        return def.Version == occurrence.Version;
    }

    private DateTime? EvaluateNextOccurrence(RequestDefinitions requestDefinitions)
    {
        try
        {
            var cron = CronExpression.Parse(requestDefinitions.CronSchedule, CronFormat.IncludeSeconds);
            var nextOccurrence = cron.GetNextOccurrence(DateTime.UtcNow);
            if (nextOccurrence.HasValue)
            {
                logger.LogDebug("Next occurrence for {RequestName} with cron '{CronSchedule}': {NextOccurrence}",
                    requestDefinitions.Name, requestDefinitions.CronSchedule, nextOccurrence.Value);
            }

            return nextOccurrence;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing cron expression '{CronSchedule}' for request {RequestName}: {Message}",
                requestDefinitions.CronSchedule, requestDefinitions.Name, ex.Message);
            return null;
        }
    }
}