using System.Text.Json;
using Messaging.Abstractions;

namespace Messaging.Pipes.Hosting
{
    /// <summary>
    /// Role-specific registry for BrowserHub.
    /// Internally delegates to HandlerRegistry.
    /// </summary>
    public class BrowserPipeRegistry : IBrowserPipeRegistry
    {
        private readonly HandlerRegistry _registry;

        public BrowserPipeRegistry(HandlerRegistry registry)
        {
            _registry = registry;
        }

        public void Register(string target, Func<JsonElement?, Task<JsonElement>> handler)
            => _registry.Register(target, handler);

        public Task<JsonElement> InvokeAsync(string target, JsonElement? payload)
            => _registry.InvokeAsync(target, payload);
    }
}
