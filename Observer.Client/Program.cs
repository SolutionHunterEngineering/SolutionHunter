// Observer.Client/Program.cs
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Observer.Client;
using Observer.Client.Services;

namespace Observer.Client;

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        // Root components
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // --------------------------------------------
        // HttpClient: point to the *Observer Server* API base
        // --------------------------------------------
        // Use appsettings.Development.json if present; else default to localhost:7288
        var apiBase = builder.Configuration["ObserverServer:BaseUrl"]
                      ?? "https://localhost:7288";

        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(apiBase)
        });

        // Auth plumbing
        // builder.Services.AddAuthorizationCore();
        // builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

        // RealtimeService: ONE PER BROWSER (Option A)
        builder.Services.AddSingleton<RealtimeService>();

        var host = builder.Build();

        // IMPORTANT: we do NOT auto-connect RealtimeService here.
        // Hunter connections are created on-demand from the NavBar.

        await host.RunAsync();
    }
}
