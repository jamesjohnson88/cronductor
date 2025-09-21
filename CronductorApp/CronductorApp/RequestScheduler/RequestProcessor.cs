using System.Net.Http.Headers;

namespace CronductorApp.RequestScheduler;

public class RequestProcessor
{
    private readonly HttpClient _client;
    private readonly ILogger<RequestProcessor> _logger;

    public RequestProcessor(HttpClient client, ILogger<RequestProcessor> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task ProcessRequest(Models.RequestDefinitions requestDefinitions)
    {
        try
        {
            _logger.LogInformation("Processing scheduled request: {RequestName}", requestDefinitions.Name);

            var response = await _client.SendAsync(CreateHttpRequestMessage(requestDefinitions), CancellationToken.None);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully processed request {RequestName} - Status: {StatusCode}",
                    requestDefinitions.Name, response.StatusCode);
            }
            else
            {
                _logger.LogWarning("Failed to process request {RequestName} - Status: {StatusCode}, Reason: {Reason}",
                    requestDefinitions.Name, response.StatusCode, response.ReasonPhrase);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for {RequestName}: {Message}", requestDefinitions.Name, ex.Message);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout for {RequestName}: {Message}", requestDefinitions.Name, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing request {RequestName}: {Message}", requestDefinitions.Name, ex.Message);
        }
    }

    private static HttpRequestMessage CreateHttpRequestMessage(Models.RequestDefinitions requestDefinitions)
    {
        var rq = new HttpRequestMessage(HttpMethod.Parse(requestDefinitions.Method), requestDefinitions.Url)
        {
            Content = requestDefinitions.Body as HttpContent ?? JsonContent.Create(requestDefinitions.Body)
        };

        if (requestDefinitions.ContentType is not null)
        {
            rq.Content.Headers.ContentType = new MediaTypeHeaderValue(requestDefinitions.ContentType);
        }

        foreach (var h in requestDefinitions.Headers)
        {
            rq.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }

        return rq;
    }
}