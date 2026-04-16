namespace Messaging.Abstractions
{
    /// <summary>
    /// IPipeReceiver
    /// -------------
    /// Abstract interface for inbound pipe listeners.
    ///
    /// Implementations (like <see cref="Pipes.PipeReceiver"/>) will:
    ///   - Bind to a named pipe.
    ///   - Receive & deserialize incoming DTOs.
    ///   - Hand them to some handler or invoker.
    ///
    /// "One side of the bridge" in Hunter.
    /// </summary>
    public interface IPipeReceiver
    {
        Task<TransportDTO> ReceiveAsync(Guid requestId, CancellationToken ct = default);
        Task StartAsync(CancellationToken ct);
        void Stop();
    }
}
