using System.Text.Json;

namespace Messaging.Abstractions
{
    /// <summary>
    /// Defines a registry of function handlers that are keyed by string identifiers.
    /// Each key corresponds to an async JSON delegate.
    /// </summary>
    public interface IPipeTargetRegistry
    {
        /// <summary>
        /// Register a new handler.
        /// Example: Register("Core.LogMessage", async (payload) => { ... return JsonElement; });
        /// </summary>
        void Register(string targetKey, Func<JsonElement?, Task<JsonElement>> handler);

        /// <summary>
        /// Invoke the handler associated with the given target key.
        /// </summary>
        Task<JsonElement> InvokeAsync(string targetKey, JsonElement? payload);
    }
}
