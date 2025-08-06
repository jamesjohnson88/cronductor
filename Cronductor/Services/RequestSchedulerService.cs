using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Cronductor.Services
{
    public class RequestGeneratorConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public Dictionary<string, string> Headers { get; set; } = new();
        public TimeSpan Frequency { get; set; } = TimeSpan.FromMinutes(1);
        public double JitterSeconds { get; set; } = 0;
        // Add more fields as needed
    }

    public class RequestLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string GeneratorName { get; set; } = "";
        public string Result { get; set; } = "";
    }

    public class RequestSchedulerService : IHostedService, IDisposable
    {
        private readonly ILogger<RequestSchedulerService> _logger;
        private readonly List<RequestGeneratorConfig> _generators = new();
        private readonly ConcurrentBag<RequestLogEntry> _log = new();
        private readonly List<Timer> _timers = new();
        private readonly HttpClient _httpClient = new();
        private bool _disposed = false;

        public RequestSchedulerService(ILogger<RequestSchedulerService> logger)
        {
            _logger = logger;
            // Example generator. In a real app, populate from config or UI.
            _generators.Add(new RequestGeneratorConfig
            {
                Name = "Example GET",
                Endpoint = "https://postman-echo.com/get",
                Method = HttpMethod.Get,
                Frequency = TimeSpan.FromSeconds(30)
            });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RequestSchedulerService starting.");
            foreach (var gen in _generators)
            {
                var timer = new Timer(async _ => await GenerateRequest(gen), null, TimeSpan.Zero, gen.Frequency);
                _timers.Add(timer);
            }
            return Task.CompletedTask;
        }

        private async Task GenerateRequest(RequestGeneratorConfig config)
        {
            try
            {
                var req = new HttpRequestMessage(config.Method, config.Endpoint);
                foreach (var hdr in config.Headers)
                    req.Headers.Add(hdr.Key, hdr.Value);

                var resp = await _httpClient.SendAsync(req);
                var body = await resp.Content.ReadAsStringAsync();
                var entry = new RequestLogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    GeneratorName = config.Name,
                    Result = $"Status: {resp.StatusCode}, Body: {body.Substring(0, Math.Min(100, body.Length))}..."
                };
                _log.Add(entry);
                _logger.LogInformation($"[{config.Name}] {entry.Result}");
            }
            catch (Exception ex)
            {
                _log.Add(new RequestLogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    GeneratorName = config.Name,
                    Result = $"ERROR: {ex.Message}"
                });
                _logger.LogError(ex, $"[{config.Name}] Request failed");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RequestSchedulerService stopping.");
            foreach (var timer in _timers)
                timer.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public IEnumerable<RequestLogEntry> GetLogs() => _log.ToArray();

        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var timer in _timers)
                    timer.Dispose();
                _httpClient.Dispose();
                _disposed = true;
            }
        }
    }
}