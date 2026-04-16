using System;
using System.Threading.Tasks;


namespace Messaging.Abstractions;

public interface ITransportServer
{
    // Register a handler the server will invoke when TargetFunction matches
    void Register(string functionName, Func<TransportDTO, Task<TransportDTO>> handler);
}
