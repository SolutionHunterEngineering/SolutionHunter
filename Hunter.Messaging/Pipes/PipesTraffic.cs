using System.Collections.Concurrent;

namespace Messaging.Transport.Hosting;

public class PipesTraffic
{
    /// <summary>
    /// Traffic - maintains a list of all CURRENT pipes activity in THIS SERVER
    ///     Guid is the unique PipeID
    ///     object is the DTO
    ///  When a pipe is sent the Dict record is created with the SENDER's Guid that is also contained
    ///     in the object (DTO).
    ///  When the pipe message arrives at the pipe Receiver, only then will isFireAndForget result in an
    ///     immediate call to remove it from the dictionary.  Otherwise, the target function will be
    ///     executed and then a new ONE-WAY pipe message will go back to the Sender.  When the original sender
    ///     receives this, the object DTO will contain the originating GUID along with the Result ReturnData.
    ///     At shis point, the record will be deleted from the dictionary and the original caller will
    ///     process the return data.
    /// 
    /// </summary>
    public ConcurrentDictionary<Guid, object> Traffic { get; set; }

    public void RegisterNewPipe(Guid pipeId, object traffic)
    {
        if (!Traffic.TryAdd(pipeId, traffic))
        {
            throw new InvalidOperationException($"Traffic entry for pipeId {pipeId} already exists.");
            //  LOG MESSAGE
        }
    }
}
