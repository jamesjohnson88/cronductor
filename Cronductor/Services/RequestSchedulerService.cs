using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Cronductor.Services
{
    public class RequestGeneratorConfig
    {
        public string Name { get; set; } = "";
        public string Endpoint { get; set; } = "";
        public HttpMethod Method { get; set; } = HttpMethod.Post;
        public TimeSpan Frequency { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan? FutureOffset { get; set; }
        public string? RequestBodyTemplate { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
    }

    public class RequestSchedulerService : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly List<RequestGeneratorConfig> _generators = new();
        private readonly List<string> _log = new();

        public RequestSchedulerService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

            // Example generator with JSON body, scheduled_for replacement
            _generators.Add(new RequestGeneratorConfig
            {
                Name = "Future Scheduled Task",
                Endpoint = "https://api.example.com/update/status",
                Method = HttpMethod.Post,
                Frequency = TimeSpan.FromMinutes(1),
                FutureOffset = TimeSpan.FromMinutes(10),
                RequestBodyTemplate = @"{
  \"scheduled_for\": 1754071000,
  \"task_request_method\": \"PUT\",
  \"task_request_url\": \"https://api.example.com/update/status\",
  \"task_request_headers\": {
    \"Accept\": \"application/json\"
  },
  \"task_request_payload\": {
    \"status\": \"active\"
  }
}"
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var nextRunTimes = new Dictionary<RequestGeneratorConfig, DateTime>();

            foreach (var gen in _generators)
                nextRunTimes[gen] = DateTime.UtcNow;

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                foreach (var gen in _generators)
                {
                    if (now >= nextRunTimes[gen])
                    {
                        _ = Task.Run(() => SendRequestAsync(gen), stoppingToken);
                        nextRunTimes[gen] = now.Add(gen.Frequency);
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task SendRequestAsync(RequestGeneratorConfig config)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(config.Method, config.Endpoint);

                // Set headers
                if (config.Headers != null)
                {
                    foreach (var kvp in config.Headers)
                    {
                        request.Headers.Add(kvp.Key, kvp.Value);
                    }
                }

                // Prepare body if present
                if (!string.IsNullOrWhiteSpace(config.RequestBodyTemplate))
                {
                    var epoch = DateTimeOffset.UtcNow.Add(config.FutureOffset ?? TimeSpan.Zero).ToUnixTimeSeconds();
                    string body = ReplaceScheduledForEpoch(config.RequestBodyTemplate, epoch);
                    request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                }

                var response = await httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();

                lock (_log)
                {
                    _log.Add($"{DateTime.UtcNow:u} [{config.Name}] {response.StatusCode} {responseText}");
                    if (_log.Count > 100) _log.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                lock (_log)
                {
                    _log.Add($"{DateTime.UtcNow:u} [{config.Name}] ERROR {ex.Message}");
                    if (_log.Count > 100) _log.RemoveAt(0);
                }
            }
        }

        private string ReplaceScheduledForEpoch(string json, long epoch)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var dict = new Dictionary<string, object>();
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.NameEquals("scheduled_for"))
                        dict["scheduled_for"] = epoch;
                    else
                        dict[prop.Name] = prop.Value.ValueKind switch
                        {
                            JsonValueKind.Object => JsonSerializer.Deserialize<object>(prop.Value.GetRawText()),
                            JsonValueKind.Array => JsonSerializer.Deserialize<object>(prop.Value.GetRawText()),
                            JsonValueKind.String => prop.Value.GetString() ?? "",
                            JsonValueKind.Number => prop.Value.GetRawText(),
                            JsonValueKind.True => true,
                            JsonValueKind.False => false,
                            JsonValueKind.Null => null,
                            _ => prop.Value.GetRawText()
                        };
                }
                return JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                // Fallback to regex for MVP; not recommended for production
                return System.Text.RegularExpressions.Regex.Replace(
                    json,
                    "\"scheduled_for\"\s*:\s*\d+",
                    $"\"scheduled_for\": {epoch}"
                );
            }
        }

        public IReadOnlyList<string> GetLog() => _log.AsReadOnly();
    }
}