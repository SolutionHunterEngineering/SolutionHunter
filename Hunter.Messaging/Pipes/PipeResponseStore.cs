using System.Collections.Concurrent;
using Messaging.Abstractions;

namespace Messaging.Transport
{
    /// <summary>
    /// Default inâ€‘memory implementation of <see cref="IPipeResponseStore"/>.
    ///
    /// Purpose:
    ///   â€¢ When a request that expects a response is sent via PipeSender,
    ///     we register its RequestId here along with a TaskCompletionSource.
    ///
    ///   â€¢ When the corresponding response arrives via PipeReceiver,
    ///     this store is signaled to complete the Task with either:
    ///       - a successful TransportDTO response
    ///       - or an Exception if the response indicated an error.
    ///
    /// Threadâ€‘safety:
    ///   Uses <see cref="ConcurrentDictionary{TKey,TValue}"/> to ensure multiple
    ///   concurrent requests can be tracked and completed safely.
    ///
    /// Correlation Strategy:
    ///   Uses Guid-based RequestId for guaranteed uniqueness across long-lived
    ///   distributed workflows. No risk of collision or wraparound.
    /// </summary>
    public sealed class PipeResponseStore : IPipeResponseStore
    {
        /// <summary>
        /// Holds all inâ€‘flight requests, keyed by RequestId (Guid).
        /// Each value is a TaskCompletionSource that the original
        /// sender is awaiting.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<TransportDTO>> _pending
            = new();

        /// <summary>
        /// Wait for a response to arrive for the given correlation ID.
        /// This method registers a TaskCompletionSource and returns its Task,
        /// which will be completed when StoreResponse is called with a matching RequestId.
        /// </summary>
        /// <param name="correlationId">The unique RequestId to wait for.</param>
        /// <param name="ct">Cancellation token to cancel the wait operation.</param>
        /// <returns>A Task that completes when the response arrives.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the same RequestId is already being waited on.
        /// </exception>
        public Task<TransportDTO> WaitForResponseAsync(Guid correlationId, CancellationToken ct = default)
        {
            var tcs = new TaskCompletionSource<TransportDTO>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            if (!_pending.TryAdd(correlationId, tcs))
            {
                throw new InvalidOperationException(
                    $"RequestId {correlationId} is already being waited on.");
            }

            // Handle cancellation by removing the pending request and canceling the task
            if (ct != default)
            {
                ct.Register(() =>
                {
                    if (_pending.TryRemove(correlationId, out var source))
                    {
                        source.TrySetCanceled();
                    }
                });
            }

            return tcs.Task;
        }

        /// <summary>
        /// Store a response and complete any waiting task for the RequestId.
        /// This method looks up the TaskCompletionSource by the response's RequestId
        /// and completes it with either the response data or an exception.
        /// </summary>
        /// <param name="response">The response DTO containing the RequestId and data.</param>
        public void StoreResponse(TransportDTO response)
        {
            var requestId = response.RequestId;

            if (_pending.TryRemove(requestId, out var tcs))
            {
                if (!string.IsNullOrEmpty(response.Error))
                {
                    // Response indicated an error â†’ fault the awaiting Task
                    tcs.TrySetException(new Exception(response.Error));
                }
                else
                {
                    // Complete the task with the response
                    tcs.TrySetResult(response);
                }
            }
            // If no matching registration found: ignore silently
            // (could be timeout/disposed caller or duplicate response)
        }
    }
}
