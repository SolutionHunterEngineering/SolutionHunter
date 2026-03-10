namespace Messaging.Abstractions
{
    /// <summary>
    /// IPipeService
    /// -------------
    /// High-level abstraction for sending requests over the Hunter pipe transport.
    ///
    /// Responsibilities:
    ///   - Take a <see cref="TransportDTO"/> request prepared by caller.
    ///   - Send it to the configured pipe transport.
    ///   - If required, wait for a matching response (CorrelationId).
    ///
    /// This isolates consumers from the low-level details of
    /// pipe send/receive, logging, or response store handling.
    /// </summary>
    public interface IPipeService
    {
        /// <summary>
        /// Sends a DTO request, optionally awaiting a correlated response.
        /// </summary>
        Task<TransportDTO> SendRequestAsync(TransportDTO dto, CancellationToken ct = default);
    }
}
