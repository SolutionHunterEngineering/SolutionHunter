namespace Messaging.Abstractions
{
    /// <summary>
    /// IPipeSender
    /// -----------
    /// Abstracts the act of serializing and physically writing DTOs
    /// to a pipe transport (NamedPipes).
    ///
    /// A "low-level" dependency used by the higher-level IPipeService.
    /// </summary>
    public interface IPipeSender
    {
        /// <summary>
        /// Write the provided DTO out through a pipe.
        /// </summary>
        Task SendAsync(TransportDTO dto, CancellationToken ct = default);
    }
}
