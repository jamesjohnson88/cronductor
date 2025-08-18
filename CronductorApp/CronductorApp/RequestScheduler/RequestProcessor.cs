using System.Net.Http.Headers;
using CronductorApp.RequestScheduler.Models;

namespace CronductorApp.RequestScheduler;

public class RequestProcessor (HttpClient client)
{
    public async Task ProcessRequest(ScheduledRequest request, CancellationToken cancellationToken)
    {
        var rq = new HttpRequestMessage(HttpMethod.Parse(request.Method), request.Url)
        {
            Content = request.Body as HttpContent ?? JsonContent.Create(request.Body)
        };
        // todo - headers, auth, etc.
        
        var response = await client.GetAsync($"https://example.com/api/{request.Name}");
        
        if (response.IsSuccessStatusCode)
        {
            // Handle successful response
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Processed request {request.Name} successfully: {content}");
        }
        else
        {
            // Handle error response
            Console.WriteLine($"Failed to process request {request.Name}: {response.ReasonPhrase}");
        }
    }
}