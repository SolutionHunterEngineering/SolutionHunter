using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using Messaging.Abstractions;

namespace Messaging.Transport.Hosting
{
    /// <summary>
    /// PipeFunctionInvoker dynamically dispatches incoming pipe messages to registered handlers.
    /// 
    /// Purpose:
    ///   â€¢ Receives function name + payload from PipeReceiver
    ///   â€¢ Looks up the registered handler for that function
    ///   â€¢ Invokes the handler and returns the result
    ///   â€¢ Handles both sync and async registered functions
    ///
    /// Registration Pattern:
    ///   invoker.RegisterHandler("GetBalance", async (payload) => await balanceService.GetBalanceAsync(payload));
    ///   invoker.RegisterHandler("LogMessage", (payload) => logger.LogInfo(payload.ToString()));
    ///
    /// Thread-safety:
    ///   Uses ConcurrentDictionary for handler storage, supports concurrent invocations.
    /// </summary>
    public sealed class PipeFunctionInvoker : IPipeFunctionInvoker
    {
        /// <summary>
        /// Registry of function name -> handler delegate mappings.
        /// Handlers can be sync or async, returning object? or Task&lt;object?&gt;.
        /// </summary>
        private readonly ConcurrentDictionary<string, Func<JsonElement, Task<object?>>> _handlers
            = new();

        /// <summary>
        /// Register a synchronous handler function.
        /// </summary>
        /// <param name="functionName">The function name that will be used in pipe messages.</param>
        /// <param name="handler">Synchronous handler that takes JsonElement and returns object?.</param>
        /// <exception cref="ArgumentException">Thrown if functionName is null/empty or already registered.</exception>
        public void RegisterHandler(string functionName, Func<JsonElement, object?> handler)
        {
            if (string.IsNullOrWhiteSpace(functionName))
                throw new ArgumentException("Function name cannot be null or empty.", nameof(functionName));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            // Wrap sync handler in async wrapper
            var asyncWrapper = new Func<JsonElement, Task<object?>>(payload => 
                Task.FromResult(handler(payload)));

            if (!_handlers.TryAdd(functionName, asyncWrapper))
                throw new ArgumentException($"Handler for '{functionName}' is already registered.", nameof(functionName));
        }

        /// <summary>
        /// Register an asynchronous handler function.
        /// </summary>
        /// <param name="functionName">The function name that will be used in pipe messages.</param>
        /// <param name="handler">Async handler that takes JsonElement and returns Task&lt;object?&gt;.</param>
        /// <exception cref="ArgumentException">Thrown if functionName is null/empty or already registered.</exception>
        public void RegisterHandler(string functionName, Func<JsonElement, Task<object?>> handler)
        {
            if (string.IsNullOrWhiteSpace(functionName))
                throw new ArgumentException("Function name cannot be null or empty.", nameof(functionName));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (!_handlers.TryAdd(functionName, handler))
                throw new ArgumentException($"Handler for '{functionName}' is already registered.", nameof(functionName));
        }

        /// <summary>
        /// Unregister a previously registered handler.
        /// </summary>
        /// <param name="functionName">The function name to unregister.</param>
        /// <returns>True if the handler was found and removed, false otherwise.</returns>
        public bool UnregisterHandler(string functionName)
        {
            if (string.IsNullOrWhiteSpace(functionName))
                return false;

            return _handlers.TryRemove(functionName, out _);
        }

        /// <summary>
        /// Get all currently registered function names.
        /// </summary>
        /// <returns>Array of registered function names.</returns>
        public string[] GetRegisteredFunctions()
        {
            return _handlers.Keys.ToArray();
        }

        /// <summary>
        /// Invoke a registered function by name with the provided payload.
        /// This is called by PipeReceiver when a message arrives.
        /// </summary>
        /// <param name="functionName">The name of the function to invoke.</param>
        /// <param name="payload">The JSON payload to pass to the function.</param>
        /// <returns>The result returned by the invoked function.</returns>
        /// <exception cref="ArgumentException">Thrown if functionName is null/empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no handler is registered for the function name.</exception>
        public async Task<object?> InvokeAsync(string functionName, JsonElement payload)
        {
            if (string.IsNullOrWhiteSpace(functionName))
                throw new ArgumentException("Function name cannot be null or empty.", nameof(functionName));

            if (!_handlers.TryGetValue(functionName, out var handler))
            {
                throw new InvalidOperationException(
                    $"No handler registered for function '{functionName}'. " +
                    $"Available functions: [{string.Join(", ", _handlers.Keys)}]");
            }

            try
            {
                // Invoke the handler (already wrapped as async)
                var result = await handler(payload).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                // Wrap handler exceptions with context
                throw new InvalidOperationException(
                    $"Error invoking function '{functionName}': {ex.Message}", ex);
            }
        }
    }
}
