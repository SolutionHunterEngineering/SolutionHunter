using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Hunter.Client;
using Hunter.Client.Configuration;
using Hunter.Client.Services;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Load client config from wwwroot/appsettings.json
using var bootHttp = new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
};

var appSettings = await bootHttp.GetFromJsonAsync<AppClientOptions>("appsettings.json")
    ?? throw new InvalidOperationException("Could not load wwwroot/appsettings.json");

// Keep config available through DI
builder.Services.AddSingleton(appSettings);

// Named HttpClient for API calls
builder.Services.AddHttpClient("Api", (sp, client) =>
{
    var cfg = sp.GetRequiredService<AppClientOptions>();

    if (string.IsNullOrWhiteSpace(cfg.ApiBaseUrl))
        throw new InvalidOperationException("ApiBaseUrl is not configured in wwwroot/appsettings.json");

    client.BaseAddress = new Uri(cfg.ApiBaseUrl);
});

// Default HttpClient for static/client-origin requests
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddSingleton<RealtimeService>();

var host = builder.Build();

var realtime = host.Services.GetRequiredService<RealtimeService>();
await realtime.StartAsync();

await host.RunAsync();