using System.Threading;
using System.Threading.Tasks;

namespace Messaging.Abstractions;

public interface ITransportClient
{
    // Send a request and expect a response envelope back
    Task<TransportDTO> RequestAsync(TransportDTO request, CancellationToken ct = default);

    // Fire-and-forget (no response expected)
    Task FireAndForgetAsync(TransportDTO request, CancellationToken ct = default);
}
