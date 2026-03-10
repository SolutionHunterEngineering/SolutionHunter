using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Http;
using Hunter.Client;
using Hunter.Client.Configuration;
using Hunter.Client.Services;
using System.Net.Http.Json;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Hunter.Client.Configuration; 

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Load appsettings.json from wwwroot
using var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var appSettings = await http.GetFromJsonAsync<AppClientOptions>("appsettings.json");

// Register options
builder.Services.AddSingleton(appSettings ?? new AppClientOptions());
builder.Services.AddSingleton<RealtimeService>();

// Register a named HttpClient for the API using ApiBase
builder.Services.AddHttpClient("Api", (sp, client) =>
{
    var cfg = sp.GetRequiredService<AppClientOptions>();
    if (string.IsNullOrWhiteSpace(cfg.ApiBase))
        throw new InvalidOperationException("ApiBase is not configured in wwwroot/appsettings.json");
    client.BaseAddress = new Uri(cfg.ApiBase);
});

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var host = builder.Build();

var realtime = host.Services.GetRequiredService<RealtimeService>();
await realtime.StartAsync();

await host.RunAsync();
