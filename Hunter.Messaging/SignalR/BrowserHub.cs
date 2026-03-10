using Microsoft.AspNetCore.SignalR;
using Messaging.Abstractions;
using System.Text.Json;

namespace Hunter.Discovery.Messaging.SignalR
{
    public class BrowserHub : Hub
    {
        private readonly IBrowserPipeRegistry _registry;

        public BrowserHub(IBrowserPipeRegistry registry)
        {
            _registry = registry;
        }

        public async Task<JsonElement> SendMessage(string target, JsonElement payload)
        {
            return await _registry.InvokeAsync(target, payload);
        }
    }
}
