using Messaging.Pipes.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Messaging.Transport.Hosting;

public sealed class MessagingHostedService : IHostedService, IDisposable
{
    private readonly ILogger<MessagingHostedService> _log;
    private readonly PipeBootstrap _bootstrap;
    private readonly NamedPipeServer _server;

    private CancellationTokenSource? _cts;
    private Task? _serverTask;

    public MessagingHostedService(
        ILogger<MessagingHostedService> log,
        PipeBootstrap bootstrap,
        NamedPipeServer server)
    {
        _log = log;
        _bootstrap = bootstrap;
        _server = server;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Messaging: startingâ€¦");

        // create our own CTS so we can stop independently
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // 1) Create/prepare all pipes, register handlers, etc.
        //    Use your real method names here.
        await _bootstrap.StartAsync(_cts.Token);    // or _bootstrap.Initialize();

        // 2) Start the long-running server loop
        //    If server already runs async, await that here; otherwise spin it in the background.
        _serverTask = _server.RunAsync(_cts.Token);   // RunAsync already returns the long-running task


        _log.LogInformation("Messaging: started.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Messaging: stoppingâ€¦");

        try { _cts?.Cancel(); } catch { /* ignore */ }

        // Tell server to stop if you have an explicit stop API
        await _server.StopAsync();


        if (_serverTask is not null)
        {
            try { await Task.WhenAny(_serverTask, Task.Delay(TimeSpan.FromSeconds(5), cancellationToken)); }
            catch { /* ignore */ }
        }

        _log.LogInformation("Messaging: stopped.");
    }

    public void Dispose()
    {
        _cts?.Dispose();
    }
}
