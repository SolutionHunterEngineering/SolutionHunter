using Microsoft.AspNetCore.SignalR;
using Messaging.Abstractions;
using System.Text.Json;

namespace Hunter.Discovery.Messaging.SignalR
{
    public class ObserverHub : Hub
    {
        private readonly IObserverPipeRegistry _registry;

        public ObserverHub(IObserverPipeRegistry registry)
        {
            _registry = registry;
        }

        public async Task<JsonElement> SendMessage(string target, JsonElement payload)
        {
            return await _registry.InvokeAsync(target, payload);
        }
    }
}
