using System.Net.Http.Headers;
using CronductorApp.RequestScheduler.Models;

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

    public async Task ProcessRequest(ScheduledRequest request)
    {
        try
        {
            _logger.LogInformation("Processing scheduled request: {RequestName}", request.Name);

            var response = await _client.SendAsync(CreateHttpRequestMessage(request), CancellationToken.None);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully processed request {RequestName} - Status: {StatusCode}",
                    request.Name, response.StatusCode);
            }
            else
            {
                _logger.LogWarning("Failed to process request {RequestName} - Status: {StatusCode}, Reason: {Reason}",
                    request.Name, response.StatusCode, response.ReasonPhrase);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for {RequestName}: {Message}", request.Name, ex.Message);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout for {RequestName}: {Message}", request.Name, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing request {RequestName}: {Message}", request.Name, ex.Message);
        }
    }

    private static HttpRequestMessage CreateHttpRequestMessage(ScheduledRequest request)
    {
        var rq = new HttpRequestMessage(HttpMethod.Parse(request.Method), request.Url)
        {
            Content = request.Body as HttpContent ?? JsonContent.Create(request.Body)
        };

        if (request.ContentType is not null)
        {
            rq.Content.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType);
        }

        foreach (var (key, value) in request.Headers)
        {
            rq.Headers.TryAddWithoutValidation(key, value);
        }

        return rq;
    }
}