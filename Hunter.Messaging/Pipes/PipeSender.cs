using System;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Messaging.Abstractions;

namespace Messaging.Transport
{
    /// <summary>
    /// The PipeSender is responsible for sending <see cref="TransportDTO"/> messages
    /// through the named pipe to the corresponding receiver.
    ///
    /// Supports two modes:
    ///   1. Fire-and-forget (no response expected, returns immediately).
    ///   2. Request/response (registers with response store and waits for correlation).
    /// 
    /// Thread-safety:
    ///   Each SendAsync call creates its own pipe connection, so multiple
    ///   concurrent sends are supported.
    /// </summary>
    public sealed class PipeSender : IPipeSender
    {
        private readonly IPipeResponseStore _responseStore;

        /// <summary>
        /// Construct a sender with an injected response store for correlation.
        /// </summary>
        /// <param name="responseStore">Store for tracking pending responses.</param>
        public PipeSender(IPipeResponseStore responseStore)
        {
            _responseStore = responseStore ?? throw new ArgumentNullException(nameof(responseStore));
        }

        /// <summary>
        /// Send a DTO message through the named pipe.
        /// 
        /// For fire-and-forget messages: writes to pipe and returns immediately.
        /// For request/response messages: registers with response store, writes to pipe,
        /// then waits for the correlated response to arrive.
        /// </summary>
        /// <param name="dto">The fully populated transport message.</param>
        /// <param name="ct">Cancellation token for timeout / cancellation.</param>
        /// <returns>A Task that completes when the message is sent (and response received if expected).</returns>
        /// <exception cref="ArgumentNullException">Thrown if dto is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if pipe connection fails.</exception>
        public async Task SendAsync(TransportDTO dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // For request/response pattern: register with response store BEFORE sending
            Task<TransportDTO>? responseTask = null;
            if (!dto.IsResponse && dto.ExpectsResponse)
            {
                // Register our correlation ID and get a task that completes when response arrives
                responseTask = _responseStore.WaitForResponseAsync(dto.RequestId, ct);
            }

            // Serialize the DTO to JSON
            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            // Convert to bytes for pipe transmission
            var bytes = Encoding.UTF8.GetBytes(json);

            try
            {
                // Create and connect to the named pipe
                using var client = new NamedPipeClientStream(
                    ".",                    // local machine
                    "SolutionHunter",       // pipe name (must match receiver)
                    PipeDirection.Out,      // write-only from sender perspective
                    PipeOptions.Asynchronous);

                // Connect to the server pipe with timeout
                await client.ConnectAsync(ct).ConfigureAwait(false);

                // Write the serialized message
                await client.WriteAsync(bytes, ct).ConfigureAwait(false);
                await client.FlushAsync(ct).ConfigureAwait(false);

                // For fire-and-forget: we're done after writing
                if (responseTask == null)
                    return;

                // For request/response: wait for the correlated response
                var response = await responseTask.ConfigureAwait(false);
                
                // Response is now available in the response store
                // The caller can retrieve it via the response store if needed
            }
            catch (TimeoutException)
            {
                throw new InvalidOperationException("Failed to connect to named pipe within timeout period.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send message via named pipe: {ex.Message}", ex);
            }
        }
    }
}
