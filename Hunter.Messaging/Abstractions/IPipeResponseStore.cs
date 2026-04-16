using System.Threading;
using System.Threading.Tasks;

namespace Messaging.Abstractions
{
    /// <summary>
    /// IPipeResponseStore
    /// ------------------
    /// Correlation store for matching request/response DTOs.
    ///
    /// - When PipeService sends a request (TransportDTO with CorrelationId),
    ///   it registers that CorrelationId in this store.
    /// - When a PipeReceiver later gets a response DTO with that CorrelationId,
    ///   it wakes up the waiter.
    ///
    /// Key to enabling "request/response" over asynchronous pipes.
    /// </summary>
    public interface IPipeResponseStore
    {
        Task<TransportDTO> WaitForResponseAsync(Guid correlationId, CancellationToken ct = default);
        void StoreResponse(TransportDTO response);
    }
}
