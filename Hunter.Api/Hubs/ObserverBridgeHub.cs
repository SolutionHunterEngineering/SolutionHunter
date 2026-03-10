using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Hunter.Api.Hubs
{
    [Authorize]
    public sealed class ObserverBridgeHub : Hub
    {
        public Task<string> Ping(string msg)
        {
            // BREAKPOINT HERE
            var reply = $"Hunter Ping OK :: {msg}";
            return Task.FromResult(reply);
        }
    }
}
