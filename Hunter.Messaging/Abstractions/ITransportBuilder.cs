using Messaging.Abstractions;

namespace Messaging.Abstractions;

public interface ITransportBuilder
{
    TransportDTO Build(
        string targetProject,
        string targetFunction,
        object? arguments,
        bool isFireAndForget = false,
        int userId = 0,
        string? sender = null);    
}
