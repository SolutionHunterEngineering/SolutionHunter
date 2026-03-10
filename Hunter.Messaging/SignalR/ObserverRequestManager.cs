using System.Collections.Concurrent;
using Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Messaging.SignalR
{
    public class ObserverRequestManager : IObserverRequestManager
    {
        private readonly ILogger<ObserverRequestManager> _logger;
        private readonly ConcurrentDictionary<Guid, ObserverRequest> _requests = new();

        public ObserverRequestManager(ILogger<ObserverRequestManager> logger)
        {
            _logger = logger;
        }

        public Task EnqueueAsync(ObserverRequest request)
        {
            _requests[request.RequestId] = request;
            _logger.LogInformation("Enqueued Request={RequestId}, Engineer={Engineer}, Problem={Problem}",
                request.RequestId, request.EngineerId, request.ProblemId);
            return Task.CompletedTask;
        }

        public Task CancelAsync(Guid requestId)
        {
            if (_requests.TryRemove(requestId, out var removed))
            {
                _logger.LogWarning("Cancelled Request={RequestId} (Engineer={Engineer}, Problem={Problem})",
                    removed.RequestId, removed.EngineerId, removed.ProblemId);
            }
            else
            {
                _logger.LogWarning("Cancel attempted for unknown Request={RequestId}", requestId);
            }
            return Task.CompletedTask;
        }

        public bool TryGet(Guid requestId, out ObserverRequest? request)
        {
            return _requests.TryGetValue(requestId, out request);
        }
    }
}
