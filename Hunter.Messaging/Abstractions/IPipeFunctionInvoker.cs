using System.Text.Json;
using System.Threading.Tasks;

namespace Messaging.Abstractions
{
    /// <summary>
    /// IPipeFunctionInvoker
    /// --------------------
    /// Provides a way to dynamically dispatch incoming messages by function name.
    ///
    /// Observer sends: TargetFunction="GetBalance"
    /// PipeReceiver calls:
    ///    invoker.InvokeAsync("GetBalance", payload)
    ///
    /// Implementation: uses reflection / registry of delegates
    /// to call the right local method.
    /// </summary>
    public interface IPipeFunctionInvoker
    {
        Task<object?> InvokeAsync(string targetFunction, JsonElement payload);
    }
}
