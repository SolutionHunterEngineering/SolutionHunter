using System.Text.Json;
using Messaging.Abstractions;

namespace Messaging.Pipes.Hosting
{
    /// <summary>
    /// Core implementation of IPipeTargetRegistry.
    /// Provides registration and invocation of pipe handlers.
    /// </summary>
    public class HandlerRegistry : IPipeTargetRegistry
    {
        // Dictionary maps a target key -> handler function
        private readonly Dictionary<string, Func<JsonElement?, Task<JsonElement>>> _handlers = new();

        public void Register(string target, Func<JsonElement?, Task<JsonElement>> handler)
        {
            _handlers[target] = handler;
        }

        public async Task<JsonElement> InvokeAsync(string target, JsonElement? payload)
        {
            if (_handlers.TryGetValue(target, out var handler))
            {
                return await handler(payload);
            }

            throw new InvalidOperationException($"No handler registered for target '{target}'");
        }
    }
}
