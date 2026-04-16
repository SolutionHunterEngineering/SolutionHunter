namespace Messaging.Abstractions
{
    /// <summary>
    /// Defines the contract for managing Observer requests within Hunter.
    /// 
    /// Responsibilities:
    ///  - Enqueue new incoming requests from Observer clients.
    ///  - Cancel previously enqueued requests by RequestId.
    ///  - Query requests to check if they are still active.
    /// 
    /// Implementations (e.g. ObserverRequestManager in Messaging/SignalR)
    /// provide the actual in-memory or distributed storage.
    /// </summary>
    public interface IObserverRequestManager
    {
        /// <summary>
        /// Enqueues a new Observer request for processing.
        /// </summary>
        /// <param name="request">The full ObserverRequest DTO (Engineer, ProblemId, Target, Payload).</param>
        Task EnqueueAsync(ObserverRequest request);

        /// <summary>
        /// Cancels a previously queued request, if it exists.
        /// </summary>
        /// <param name="requestId">The unique request ID assigned when the request was enqueued.</param>
        Task CancelAsync(Guid requestId);

        /// <summary>
        /// Attempts to look up a request by its ID.
        /// </summary>
        /// <param name="requestId">The ID of the request to look up.</param>
        /// <param name="request">Outputs the request if it exists, otherwise null.</param>
        /// <returns>True if found, false if not.</returns>
        bool TryGet(Guid requestId, out ObserverRequest? request);
    }
}
