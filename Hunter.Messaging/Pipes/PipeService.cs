using Messaging.Abstractions; 

namespace Messaging.Pipes
{
    /// <summary>
    /// Higher-level service abstraction over the low-level pipe sender/receiver.
    /// Provides an API to send requests & receive responses via TransportDTO.
    /// </summary>
    public sealed class PipeService : IPipeService
    {
        private readonly IPipeSender _sender;
        private readonly IPipeReceiver _receiver;

        public PipeService(IPipeSender sender, IPipeReceiver receiver)
        {
            _sender = sender;
            _receiver = receiver;
        }

        /// <inheritdoc />
        public async Task<TransportDTO> SendRequestAsync(TransportDTO dto, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Ensure RequestId is set for correlation
            if (dto.RequestId == Guid.Empty)
                dto.RequestId = Guid.NewGuid();

            await _sender.SendAsync(dto, ct);

            if (dto.IsFireAndForget)
            {
                return dto; // fire-and-forget, return immediately
            }

            // Wait for correlated response through the receiver
            return await _receiver.ReceiveAsync(dto.RequestId, ct).ConfigureAwait(false);
        }
    }
}
