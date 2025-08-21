using System.Net.Http.Headers;
using CronductorApp.RequestScheduler.Models;

namespace CronductorApp.RequestScheduler;

public class RequestProcessor (HttpClient client)
{
    public async Task ProcessRequest(ScheduledRequest request)
    {
        var response = await client.SendAsync(CreateHttpRequestMessage(request), CancellationToken.None);
        if (response.IsSuccessStatusCode)
        {
            // Handle successful response
            //var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Processed request {request.Name} successfully.");
        }
        else
        {
            // Handle error response
            Console.WriteLine($"Failed to process request {request.Name}: {response.ReasonPhrase}");
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