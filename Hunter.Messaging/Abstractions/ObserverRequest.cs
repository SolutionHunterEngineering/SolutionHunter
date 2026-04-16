using System.Text.Json;

namespace Messaging.Abstractions;

public class ObserverRequest
{
    public Guid RequestId { get; init; }
    public string EngineerId { get; init; } = string.Empty;
    public string ProblemId { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public JsonElement Payload { get; init; }
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
}
