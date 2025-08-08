using System.Net.Http;
using Cronductor.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();
builder.Services.AddSingleton<RequestSchedulerService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var logHubContext = sp.GetService<Microsoft.AspNetCore.SignalR.IHubContext<Cronductor.LogHub>>();
    return new RequestSchedulerService(httpClientFactory, logHubContext);
});
builder.Services.AddHostedService(sp => sp.GetRequiredService<RequestSchedulerService>());
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapHub<Cronductor.LogHub>("/logHub");
app.MapFallbackToPage("/_Host");

app.Run();
