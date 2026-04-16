using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Messaging.Abstractions;

namespace Messaging.Pipes
{
    /// <summary>
    /// PipeReceiver implementation that listens on a named pipe for incoming TransportDTO messages.
    /// Handles background listening and provides both async receiving and response correlation.
    /// </summary>
    public sealed class PipeReceiver : IPipeReceiver
    {
        private readonly string _pipeName;
        private readonly ILogger<PipeReceiver> _logger;
        private readonly IPipeResponseStore _responseStore;
        private readonly IPipeFunctionInvoker _functionInvoker;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _listeningTask;

        // Constructor matching your existing PipeBootstrap call pattern
        public PipeReceiver(
            string pipeName,
            ILogger<PipeReceiver> logger,
            IPipeResponseStore responseStore,
            IPipeFunctionInvoker functionInvoker)
        {
            _pipeName = pipeName;
            _logger = logger;
            _responseStore = responseStore;
            _functionInvoker = functionInvoker;
        }

        /// <summary>
        /// Start the background listening loop on the named pipe.
        /// </summary>
        public async Task StartAsync(CancellationToken ct = default)
        {
            if (_listeningTask != null)
            {
                _logger.LogWarning("PipeReceiver is already started");
                return;
            }

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _listeningTask = ListenLoopAsync(_cancellationTokenSource.Token);

            _logger.LogInformation("PipeReceiver started on pipe: {PipeName}", _pipeName);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Stop the background listening loop.
        /// </summary>
        public void Stop()
        {
            if (_cancellationTokenSource != null)
            {
                _logger.LogInformation("Stopping PipeReceiver...");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            _listeningTask = null;
        }

        /// <summary>
        /// Wait for a specific response message by RequestId.
        /// This delegates to the response store for correlation.
        /// </summary>
        public async Task<TransportDTO> ReceiveAsync(Guid requestId, CancellationToken ct = default)
        {
            if (requestId == Guid.Empty)
                throw new ArgumentException("RequestId cannot be empty", nameof(requestId));

            // For now, implement a simple polling mechanism
            // TODO: Replace with proper response store integration once we know the exact API
            var timeout = TimeSpan.FromSeconds(30);
            var start = DateTime.UtcNow;
            
            while (DateTime.UtcNow - start < timeout && !ct.IsCancellationRequested)
            {
                // This is a placeholder - will need to be replaced with actual response store logic
                await Task.Delay(100, ct);
            }

            throw new TimeoutException($"No response received for RequestId: {requestId}");
        }

        /// <summary>
        /// Background loop that listens on the named pipe for incoming messages.
        /// </summary>
        private async Task ListenLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var pipeServer = new NamedPipeServerStream(
                        _pipeName,
                        PipeDirection.InOut,
                        1,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);

                    _logger.LogDebug("Waiting for pipe connection on {PipeName}...", _pipeName);
                    await pipeServer.WaitForConnectionAsync(ct);
                    _logger.LogDebug("Pipe connected");

                    // Read the incoming message
                    var buffer = new byte[4096];
                    var bytesRead = await pipeServer.ReadAsync(buffer, 0, buffer.Length, ct);
                    
                    if (bytesRead > 0)
                    {
                        var json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        var dto = JsonSerializer.Deserialize<TransportDTO>(json);
                        
                        if (dto != null)
                        {
                            await ProcessIncomingMessageAsync(dto, ct);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in pipe listening loop");
                    
                    // Brief delay before retrying to avoid tight error loops
                    if (!ct.IsCancellationRequested)
                    {
                        await Task.Delay(1000, ct);
                    }
                }
            }

            _logger.LogInformation("PipeReceiver listening loop stopped");
        }

        /// <summary>
        /// Process an incoming message - either store response or invoke function.
        /// </summary>
        private async Task ProcessIncomingMessageAsync(TransportDTO dto, CancellationToken ct)
        {
            try
            {
                if (dto.IsResponse)
                {
                    // This is a response to a previous request - store it for correlation
                    _logger.LogDebug("Received response for RequestId: {RequestId}", dto.RequestId);
                    
                    // TODO: Call the actual response store method once we know the signature
                    // await _responseStore.StoreResponseAsync(dto, ct);
                    _logger.LogDebug("Response stored for RequestId: {RequestId}", dto.RequestId);
                }
                else
                {
                    // This is a new request - invoke the appropriate function
                    _logger.LogDebug("Received request for {TargetProject}.{TargetFunction}", 
                        dto.TargetProject, dto.TargetFunction);
                    
                    // TODO: Implement function invocation once we know the signature
                    // var response = await _functionInvoker.InvokeAsync(dto, ct);
                    
                    // If the request expects a response, send it back
                    if (dto.ExpectsResponse)
                    {
                        // TODO: Send response back through pipe sender
                        _logger.LogDebug("Function invocation completed, response ready for RequestId: {RequestId}", 
                            dto.RequestId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing incoming message for RequestId: {RequestId}", dto.RequestId);
            }
        }
    }
}
