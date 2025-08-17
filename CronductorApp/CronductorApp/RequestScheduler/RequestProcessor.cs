using CronductorApp.RequestScheduler.Models;

namespace CronductorApp.RequestScheduler;

public class RequestProcessor (HttpClient client)
{
    public async Task ProcessRequest(ScheduledRequest request, CancellationToken cancellationToken)
    {
        // Here we would implement the logic to process the request
        // For example, making an HTTP call or executing some business logic
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