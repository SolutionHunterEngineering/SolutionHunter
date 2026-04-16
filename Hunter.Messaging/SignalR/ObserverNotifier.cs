using Hunter.Discovery.Messaging.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Messaging.SignalR;

namespace Messaging.SignalR
{
    public class ObserverNotifier
    {
        private readonly IHubContext<ObserverHub> _hubContext;
        private readonly ILogger<ObserverNotifier> _logger;

        public ObserverNotifier(IHubContext<ObserverHub> hubContext, ILogger<ObserverNotifier> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyObserverAsync(string observerId, object response)
        {
            _logger.LogDebug("Sending response to Observer={ObserverId}", observerId);
            await _hubContext.Clients.User(observerId).SendAsync("ObserverMessage", response);
        }
    }
}
