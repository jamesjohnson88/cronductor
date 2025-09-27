using CronductorApp.Components;
using CronductorApp.Data;
using CronductorApp.RequestScheduler;
using CronductorApp.RequestScheduler.Data;
using CronductorApp.Services;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqliteWithMigrations("Data Source=cronductor.db");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<RequestProcessor>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
    client.DefaultRequestHeaders.Add("User-Agent", "CronductorApp/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = true,
    UseProxy = true,
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
})
.AddPolicyHandler(GetRetryPolicy());

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<RequestDefinitionRepository>();
builder.Services.AddSingleton<ScheduleService>();
builder.Services.AddSingleton<RequestService>();

builder.Services.AddHostedService<BackgroundScheduler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.ApplyMigrations();

app.Run();
return;

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}