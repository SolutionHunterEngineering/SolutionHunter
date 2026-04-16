using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Messaging.Transport.Hosting;

public class NamedPipeServer
{
    private readonly string _pipeName;
    private readonly int _maxConnections;

    private readonly CancellationTokenSource _internalCts = new(); // used by Start()
    private CancellationTokenSource? _linkedCts;                   // used by RunAsync(hostToken)
    private Task? _listenTask;
    private readonly List<Task> _clientTasks = new();

    public NamedPipeServer(string pipeName, int maxConnects)
    {
        _pipeName = pipeName;
        _maxConnections = maxConnects;
    }

    // Legacy fire-and-forget start (kept for convenience)
    public void Start()
    {
        _listenTask = ListenLoopAsync(_internalCts.Token);
    }

    // Preferred: host-managed lifetime; returns the long-running task
    public Task RunAsync(CancellationToken hostToken)
    {
        _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(hostToken);
        _listenTask = ListenLoopAsync(_linkedCts.Token);
        return _listenTask;
    }

    public async Task ListenLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var serverStream = new NamedPipeServerStream(
                _pipeName,
                PipeDirection.InOut,
                _maxConnections,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            try
            {
                await serverStream.WaitForConnectionAsync(token);
            }
            catch (OperationCanceledException)
            {
                serverStream.Dispose();
                break; // exit loop on shutdown
            }

            var task = HandleClientAsync(serverStream, token);
            lock (_clientTasks) { _clientTasks.Add(task); }
            lock (_clientTasks) { _clientTasks.RemoveAll(t => t.IsCompleted); }
        }
    }

    private async Task HandleClientAsync(NamedPipeServerStream stream, CancellationToken token)
    {
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream) { AutoFlush = true };

        try
        {
            var request = await reader.ReadLineAsync();
            if (!string.IsNullOrWhiteSpace(request))
            {
                // TODO: deserialize/route/invoke â€” placeholder echo
                var response = $"Echo: {request}";
                await writer.WriteLineAsync(response);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[PipeServer] Client handling failed: {ex.Message}");
        }
    }

    public async Task StopAsync()
    {
        try { _internalCts.Cancel(); } catch { }
        try { _linkedCts?.Cancel(); } catch { }

        if (_listenTask is not null)
        {
            try { await _listenTask; } catch (OperationCanceledException) { }
        }

        Task[] pending;
        lock (_clientTasks) { pending = _clientTasks.ToArray(); }
        if (pending.Length > 0)
        {
            try { await Task.WhenAll(pending); } catch { /* ignore client faults on shutdown */ }
        }
    }
}
